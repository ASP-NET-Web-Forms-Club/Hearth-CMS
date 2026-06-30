using System.Text;
using System.engine;

namespace System.engine.RH
{
    // ============================================================
    // ArticleTools - the ONE source of truth for the article content area,
    // including the "View Content / View Markdown" toggle.
    //
    // Every renderer that emits a single post/page body calls this:
    //   - the HTML-template engine (DocLayout.RenderTemplated -> {{article_content}})
    //   - the C# themes (Broadsheet, Hearth) ArticleHtml
    // so the rendered structure is byte-for-byte identical across all themes.
    //
    // OWNERSHIP CONTRACT
    //   This builder owns the INNER content only. The single .doc-content.prose
    //   wrapper is owned by the CALLER (the theme's article template file, or the
    //   C# theme's ArticleHtml). This is why no HTML template needs editing: each
    //   article-*.html already wraps {{article_content}} in .doc-content.prose,
    //   and this builder drops its markup straight inside that wrapper.
    //
    // TOGGLE GATE
    //   The toggle is emitted only for a real post (postId > 0) AND when the admin
    //   setting Settings.ShowMarkdownButton is on (default ON). Pages (postId == 0)
    //   and the setting being off both fall through to the plain content - the
    //   caller's .doc-content.prose then renders exactly as it always did.
    //
    // FIXED STRUCTURE (documented in the General Guidelines; theme CSS matches it):
    //   <div class='md-toolbar'> ...two buttons... </div>
    //   <div id='post_content'> ...rendered HTML... </div>
    //   <div id='post_markdown' style='display:none'><textarea readonly></textarea></div>
    //   <script>const post_id = N;</script>
    //   <script> showContent() / showMarkdown() - lazy-fetches the markdown </script>
    //
    // The markdown is fetched lazily from the public, no-login endpoint
    // /api/get-article-markdown?id=N (GetArticleMarkdownApi), which both engines
    // already share. IDs are safe here because exactly one article renders per
    // request.
    // ============================================================
    public static class ArticleTools
    {
        // Build the inner content for a post/page body. Returns the plain rendered
        // HTML for a page (postId <= 0) or when the toggle is disabled; otherwise
        // wraps it in the View Content / View Markdown toggle structure above.
        public static string BuildContentArea(string content, int postId)
        {
            content = content ?? "";

            if (postId <= 0 || !Settings.ShowMarkdownButton)
                return content;

            var sb = new StringBuilder();
            // Theme-adaptive fallback styling. Emitted in a CSS cascade @layer so
            // any theme's own .md-toolbar / #post_markdown rules (which are
            // unlayered, e.g. hearth-cs, Broadsheet-cs) ALWAYS win over it - this
            // only prevents a raw, broken-looking default on themes that ship no
            // styling. Uses inherit / currentColor / transparent so it adapts to
            // light and dark themes alike. Browsers without @layer drop the block
            // and fall back to the previous unstyled behaviour.
            sb.Append(FallbackStyle);
            sb.Append(@"<div class='md-toolbar'>
<button type='button' onclick='showContent();'>View Content</button>
<button type='button' onclick='showMarkdown();'>View Markdown</button>
</div>
<div id='post_content'>
");
            sb.Append(content);
            sb.Append(@"
</div>
<div id='post_markdown' style='display:none'>
<textarea readonly></textarea>
</div>
");
            // Dynamic: the id this page fetches its raw markdown for.
            sb.AppendFormat("<script>\nconst post_id = {0};\n</script>", postId);
            // Static: the View Content / View Markdown behaviour (public API, no login).
            sb.Append(MarkdownToggleScript);
            return sb.ToString();
        }

        // Theme-adaptive fallback so the fixed structure never looks broken on a
        // theme that ships no CSS for it. In a @layer so themes override freely.
        const string FallbackStyle = @"<style>
@layer hearth-md-fallback {
    .md-toolbar { display: flex; flex-wrap: wrap; gap: 8px; margin: 0 0 16px; }
    .md-toolbar button {
        font: inherit; font-size: .85em; padding: 6px 14px;
        color: inherit; background: transparent;
        border: 1px solid currentColor; border-radius: 6px;
        cursor: pointer; opacity: .7; transition: opacity .12s;
    }
    .md-toolbar button:hover { opacity: 1; }
    #post_markdown textarea {
        width: 100%; min-height: 60vh; box-sizing: border-box;
        font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
        font-size: .9em; line-height: 1.6;
        color: inherit; background: transparent;
        border: 1px solid currentColor; border-radius: 8px;
        padding: 16px 18px; resize: vertical; opacity: .92;
    }
}
</style>
";

        // Static toggle script - identical for every post. A plain (non-interpolated)
        // verbatim string, so the JS braces and the `${post_id}` template literal are
        // emitted to the browser as-is.
        const string MarkdownToggleScript = @"<script>
const API_ENDPOINT = `/api/get-article-markdown?id=${post_id}`;

function showContent() {
    document.getElementById('post_content').style.display = 'block';
    document.getElementById('post_markdown').style.display = 'none';
}

function showMarkdown() {
    document.getElementById('post_content').style.display = 'none';
    document.getElementById('post_markdown').style.display = 'block';
    var ta = document.querySelector('#post_markdown textarea');
    ta.value = 'Loading...';
    fetch(API_ENDPOINT)
        .then(function (r) { return r.text(); })
        .then(function (md) { ta.value = md; })
        .catch(function () { ta.value = 'Failed to load markdown.'; });
}
</script>";
    }
}
