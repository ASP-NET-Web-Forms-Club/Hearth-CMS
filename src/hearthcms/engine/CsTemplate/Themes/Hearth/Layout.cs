using System.Text;
using System.Web;
using System.engine;
using System.engine.RH;

namespace System.engine.CsTemplate.Hearth
{
    // Shared layout helper for the Hearth (C#) theme: renders the document
    // head/header and footer so the page handlers only build the body. This
    // is the C# theme's equivalent of a folder theme's _layout.html, and is
    // kept structurally in lock-step with App_Data/themes/hearth/_layout.html.
    //
    // Conservative C# style (string.Format / concat, no interpolation) for consistency.
    public class Layout
    {
        public string Title = "";
        public string MetaDescription = "";

        // Per-page Open Graph image (root-relative or absolute). Set by the post
        // handler from the post's cover image; empty for the home/listing/page
        // handlers, so the engine falls back to the site default og_image_url.
        // Mirrors PublicTemplate.OgImage on the HTML (RenderPage) path.
        public string OgImage = "";

        // Public URL of the theme's asset folder (e.g. "/assets/themes/hearth-cs"),
        // passed in by the handler so CSS/JS links resolve to the right slug.
        public string AssetBaseUrl = "";

        static string Enc(string v) { return HttpUtility.HtmlEncode(v ?? ""); }
        static string AttrEnc(string v) { return HttpUtility.HtmlAttributeEncode(v ?? ""); }

        public string RenderHeader()
        {
            // {{head_meta}} equivalent: the page <title>, meta description,
            // the site-wide favicon, plus the full meta block (theme-color and
            // Open Graph / Twitter cards) - the same SiteMetaHeadTags the HTML
            // RenderPage path emits, so per-page covers win for og:image and the
            // site default og_image_url is used otherwise. SiteMetaHeadTags
            // already emits the favicon set, so it is not added separately here.
            string headMeta = "<title>" + Enc(Title) + "</title>\n    <meta name='description' content='" + Enc(MetaDescription) + "' />";
            headMeta += "\n    " + PublicTemplate.SiteMetaHeadTags(Title, MetaDescription, OgImage);

            string siteName = Settings.SiteName;

            return string.Format(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    {0}
    <link rel='stylesheet' href='/fonts/fontawesome/css/all.min.css' />
    <link rel='preconnect' href='https://fonts.googleapis.com'>
    <link rel='preconnect' href='https://fonts.gstatic.com' crossorigin>
    <link rel='stylesheet' href='https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Fraunces:opsz,wght@9..144,400;9..144,500;9..144,600;9..144,700&display=swap'>
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css'>
    <link rel='stylesheet' href='{1}/hearth.css' />
</head>
<body>
<a class='skip-link' href='#main'>Skip to content</a>
<header class='site-header'>
    <div class='container header-inner'>
        {4}
        <button class='nav-toggle' onclick='toggleNav()' aria-label='Menu' aria-controls='siteNav'>
            <i class='fa-solid fa-bars'></i>
        </button>
        <nav class='site-nav' id='siteNav'>
            {3}
        </nav>
    </div>
</header>
<div class='nav-overlay' id='navOverlay' onclick='toggleNav()'></div>
<main class='site-main' id='main'>
", headMeta, AttrEnc(AssetBaseUrl), Enc(siteName), NavMenu.RenderPublicNav(), PublicTemplate.RenderSiteLogo());
        }

        public string RenderFooter()
        {
            string footerText = HttpUtility.HtmlEncode(Settings.FooterText);

            return string.Format(@"
</main>
<footer class='site-footer'>
    <div class='container footer-cols'>{0}</div>
    <div class='container footer-inner'>
        <div class='footer-meta'>{1}</div>
    </div>
</footer>
<script src='/js/site.js'></script>
<script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js'></script>
<script>setTimeout(function(){{document.querySelectorAll('pre code').forEach(function(el){{try{{hljs.highlightElement(el);}}catch(e){{}}}});}},250);</script>
</body>
</html>", BuildFooterColumns(), footerText);
        }

        // The optional multi-column footer area, mirroring components/footer-column.html.
        // Each configured column's markdown is rendered to HTML and wrapped in a
        // .footer-col. Returns "" when disabled or every column is empty.
        static string BuildFooterColumns()
        {
            int count = Settings.FooterColCount;
            if (count < 1) return "";
            if (count > 4) count = 4;

            var sb = new StringBuilder();
            for (int i = 1; i <= count; i++)
            {
                string md = Settings.FooterColumn(i);
                if (md.Length == 0) continue;
                string html = System.engine.Markdown.MarkdownToHtml.ToHtml(md);
                sb.Append("<div class='footer-col'>").Append(html).Append("</div>");
            }
            return sb.ToString();
        }
    }
}