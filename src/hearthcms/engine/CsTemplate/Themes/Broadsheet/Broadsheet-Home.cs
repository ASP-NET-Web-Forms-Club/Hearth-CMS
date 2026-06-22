using System;
using System.Collections.Generic;
using System.Text;
using System.engine;
using System.engine.RH;

namespace System.engine.CsTemplate.Broadsheet
{
    public partial class Broadsheet : CsTemplate
    {
        // ----- home : hero + latest-posts grid + feature row -----
        //        (mirrors home.html + components/section-latest-posts.html
        //         + components/post-card.html)
        public override void HandleHome()
        {
            var layout = new Layout
            {
                Title = GetSiteName(),
                MetaDescription = GetSiteDescription(),
                AssetBaseUrl = BroadsheetAssetBase
            };

            // How many recent posts to feature (admin-tunable, same as HTML home).
            int homeCount = GetCountSetting("home_post_count");

            // Recent posts via the CsTemplate data helper (no direct SQLite here).
            var recent = GetRecentPost(homeCount);

            string siteName = GetSiteName();
            string siteTagline = GetSiteTagline("Engineering, explained in full.");
            string siteDescription = GetSiteDescription("Deep write-ups on C#, ASP.NET and systems architecture.");

            var sb = new StringBuilder();
            sb.Append(layout.RenderHeader());

            // ----- hero (mirrors home.html) -----
            sb.Append(string.Format(@"
<section class='hero'>
    <div class='container'>
        <span class='hero-eyebrow'><i class='fa-solid fa-pen-nib'></i> <span>{0}</span></span>
        <h1 class='hero-title'>{1}</h1>
        <p class='hero-sub'>{2}</p>
        <div class='hero-actions'>
            <a href='/blog' class='btn btn-primary'><span>Read the blog</span> <i class='fa-solid fa-arrow-right'></i></a>
            <a href='/about' class='btn btn-ghost'><span>About</span></a>
        </div>
    </div>
</section>
", H(siteName), H(siteTagline), H(siteDescription)));

            // ----- latest writing (mirrors components/section-latest-posts.html
            //       + components/post-card.html); omitted when there are no posts -----
            if (recent.Count > 0)
            {
                sb.Append(@"
<section class='section'>
    <div class='container'>
        <div class='section-heading'>
            <h2>Latest writing</h2>
            <a href='/blog' class='section-link'>All posts <i class='fa-solid fa-arrow-right'></i></a>
        </div>
        <div class='post-grid'>");

                for (int i = 0; i < recent.Count; i++)
                {
                    var p = recent[i];
                    string excerpt = PostExcerpt(p, 160);

                    sb.Append(string.Format(@"
            <article class='post-card'>
                <a href='/{0}' class='post-card-inner'>
                    <h3 class='post-card-title'>{1}</h3>
                    <p class='post-card-excerpt'>{2}</p>
                    <span class='post-card-meta'><i class='fa-regular fa-calendar'></i> {3}</span>
                </a>
            </article>", Attr(p.Slug), H(p.Title), H(excerpt), H(DateDisplay.Format(p.DatePublished))));
                }

                sb.Append(@"
        </div>
    </div>
</section>
");
            }

            // ----- feature row (mirrors home.html) -----
            sb.Append(@"
<section class='section section-bordered'>
    <div class='container features-grid'>
        <div class='feature'>
            <span class='feature-icon'><i class='fa-solid fa-code'></i></span>
            <h3>Built from scratch</h3>
            <p>Low-level, dependency-light engineering write-ups. The protocol itself, not a framework wrapper.</p>
        </div>
        <div class='feature'>
            <span class='feature-icon'><i class='fa-solid fa-bolt'></i></span>
            <h3>Server-rendered &amp; fast</h3>
            <p>Pageless ASP.NET Web Forms architecture &mdash; cached, quiet, and instant to load.</p>
        </div>
        <div class='feature'>
            <span class='feature-icon'><i class='fa-solid fa-layer-group'></i></span>
            <h3>Deep, not wide</h3>
            <p>C#, ASP.NET and systems architecture, explained in full with working code.</p>
        </div>
    </div>
</section>
");

            sb.Append(layout.RenderFooter());

            // Sanctioned output: through the public page cache.
            WriteCached(sb.ToString());
        }
    }
}
