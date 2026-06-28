using System.Collections.Concurrent;
using System.Text;
using System.Web;
using System.engine.Markdown;
using System.engine.CsTemplate;

namespace System.engine.RH
{
    // One-shot Markdown preview pipeline:
    //   POST /api/admin/preview-markdown  (form: token, markdown) -> stores it
    //   GET  /api/admin/preview-markdown?token=...                -> renders + removes
    // The token is consumed on GET so a leaked URL can't be reused. The store
    // also self-evicts entries older than TtlSeconds to bound memory.
    public static class AdminPreviewApi
    {
        const int TtlSeconds = 120;
        const int MaxEntries = 256;

        struct Entry { public string Markdown; public DateTime CreatedUtc; }
        static readonly ConcurrentDictionary<string, Entry> _store =
            new ConcurrentDictionary<string, Entry>();

        public static void HandleRequest()
        {
            if (!AdminGuard.RequireLoginApi()) return;

            var ctx = HttpContext.Current;
            string method = (ctx.Request.HttpMethod ?? "GET").ToUpperInvariant();

            try
            {
                if (method == "POST") Store();
                else Render();
            }
            catch (Exception ex)
            {
                ApiHelper.WriteError(ex.Message, 500);
            }
            ApiHelper.EndResponse();
        }

        static void Store()
        {
            var req = HttpContext.Current.Request;
            string token = (req.Form["token"] + "").Trim();
            string markdown = req.Form["markdown"] + "";

            if (string.IsNullOrEmpty(token) || token.Length < 8 || token.Length > 128)
            {
                ApiHelper.WriteError("Invalid token");
                return;
            }
            if (!IsTokenSafe(token))
            {
                ApiHelper.WriteError("Invalid token");
                return;
            }

            EvictExpired();
            if (_store.Count >= MaxEntries)
            {
                // Hard cap: oldest gets booted to make room.
                string oldest = null;
                DateTime oldestTs = DateTime.MaxValue;
                foreach (var kv in _store)
                {
                    if (kv.Value.CreatedUtc < oldestTs)
                    {
                        oldestTs = kv.Value.CreatedUtc;
                        oldest = kv.Key;
                    }
                }
                if (oldest != null) { Entry _; _store.TryRemove(oldest, out _); }
            }

            _store[token] = new Entry { Markdown = markdown, CreatedUtc = DateTime.UtcNow };
            ApiHelper.WriteSuccess("Stored");
        }

        static void Render()
        {
            var ctx = HttpContext.Current;
            string token = (ctx.Request.QueryString["token"] + "").Trim();

            EvictExpired();

            Entry entry;
            if (string.IsNullOrEmpty(token) || !_store.TryRemove(token, out entry))
            {
                WritePreviewHtml("<p style='color:#a00'>Preview token expired or not found. Switch back to the Edit tab and click Preview again.</p>");
                return;
            }

            string body = MarkdownToHtml.ToHtml(entry.Markdown ?? "");

            // Folder/HTML themes: render the content through the SAME pipeline a real
            // published page uses - the theme's article template wrapped in its
            // _layout.html - so the preview inherits the designer's COMPLETE <head>
            // (every stylesheet, web font and highlight.js theme), not just a single
            // CSS file we tried to guess. C# (code) themes have no reusable HTML
            // templates, so they keep the self-contained wrapper below. Any failure
            // (missing/broken theme templates) falls back to that wrapper too, so the
            // preview always shows the converted HTML rather than an error page.
            bool isCsTheme = CsThemeRegistry.IsActiveCsTemplate && CsThemeRegistry.Active != null;
            if (!isCsTheme && TryWriteTemplatedPreview(body)) return;

            WritePreviewHtml(body);
        }

        // Render the preview body through the active folder theme's real page
        // template (article-full-width.html + _layout.html). Returns false on any
        // failure so the caller can fall back to the self-contained wrapper.
        static bool TryWriteTemplatedPreview(string bodyHtml)
        {
            try
            {
                var model = new DocModel
                {
                    Title = "Preview",
                    Layout = "stack",     // full-width, no "recent posts" aside
                    ShowAside = false,
                    RenderedContentHtml = bodyHtml
                };
                string page = new PublicTemplate
                {
                    Title = "Preview",
                    BodyClass = "page-doc"
                }.RenderPage(DocLayout.RenderTemplated(model));

                if (string.IsNullOrEmpty(page)) return false;

                var ctx = HttpContext.Current;
                ctx.Response.ContentType = "text/html; charset=utf-8";
                ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                ctx.Response.Write(page);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void WritePreviewHtml(string bodyHtml)
        {
            var ctx = HttpContext.Current;
            ctx.Response.ContentType = "text/html; charset=utf-8";
            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            // Mirror PublicTemplate's <head> exactly (active theme slug and CSS)
            // so the preview inherits the same typography and CSS variables as the
            // public site. Body and wrapper classes mirror the homepage "custom
            // page" rendering (body.page-home.page-doc -> article.doc ->
            // .container.container-narrow -> .doc-content.prose) so theme rules
            // keyed off those classes (backgrounds, container widths) apply. We
            // deliberately skip site-header, nav-overlay, and site-footer -
            // chrome the editor doesn't need to preview.
            // Resolve the active theme's real stylesheet URL. A C# (code) theme
            // ships its own bundled CSS (e.g. broadsheet-cs.css) and does NOT
            // publish a {slug}/style.css, so the folder-theme convention 404s and
            // the preview renders unstyled. Use the active theme's declared CssHref
            // in that case; fall back to the folder convention for HTML themes.
            string activeThemeSlug = ThemeManager.GetActiveSlug();
            string themeHref;
            if (CsThemeRegistry.IsActiveCsTemplate && CsThemeRegistry.Active != null)
                themeHref = CsThemeRegistry.Active.CssHref;
            else
                themeHref = ThemeManager.ResolveCssPublicUrl(activeThemeSlug)
                            ?? ThemeManager.CssPublicUrl(activeThemeSlug);

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Preview</title>
    <link rel='stylesheet' href='/fonts/fontawesome/css/all.min.css' />
    <link rel='stylesheet' href='" + themeHref + @"' />
</head>
<body class='page-home page-doc'>
<main class='site-main'>
<article class='doc'>
    <div class='container container-narrow'>
        <div class='doc-content prose'>
");
            sb.Append(bodyHtml);
            sb.Append(@"
        </div>
    </div>
</article>
</main>
</body>
</html>");
            ctx.Response.Write(sb.ToString());
        }

        static void EvictExpired()
        {
            DateTime cutoff = DateTime.UtcNow.AddSeconds(-TtlSeconds);
            foreach (var kv in _store)
            {
                if (kv.Value.CreatedUtc < cutoff)
                {
                    Entry _;
                    _store.TryRemove(kv.Key, out _);
                }
            }
        }

        static bool IsTokenSafe(string t)
        {
            for (int i = 0; i < t.Length; i++)
            {
                char c = t[i];
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                          || (c >= '0' && c <= '9') || c == '-' || c == '_';
                if (!ok) return false;
            }
            return true;
        }
    }
}