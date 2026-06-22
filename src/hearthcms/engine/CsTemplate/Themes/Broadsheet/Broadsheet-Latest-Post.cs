using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.engine;
using System.engine.RH;

namespace System.engine.CsTemplate.Broadsheet
{
    public partial class Broadsheet : CsTemplate
    {
        // ----- /latest-post : flat row list + search -----
        public override void HandleLatestPost()
        {
            string q = (HttpContext.Current.Request.QueryString["q"] + "").Trim();
            int limit = GetCountSetting("latest_post_count");

            // Data via the CsTemplate helpers - no direct SQLite in theme code.
            List<obPost> posts = string.IsNullOrEmpty(q)
                ? GetRecentPost(limit)
                : SearchPosts(q, 0, limit);

            string subheading = string.IsNullOrEmpty(q)
                ? "<p class='list-sub'>Fresh writing, newest first.</p>" : "";
            string searchMeta = string.IsNullOrEmpty(q)
                ? ""
                : string.Format("<p class='search-meta'>{0} result(s) for &ldquo;{1}&rdquo; &middot; <a href='/latest-post'>Clear</a></p>",
                    posts.Count, H(q));

            var layout = NewLayout("Latest posts");
            var sb = new StringBuilder();
            sb.Append(layout.RenderHeader());
            sb.Append(string.Format(@"
<section class='section'>
    <div class='container container-narrow'>
        <div class='list-head'>
            <h1>Latest posts</h1>
            {0}
        </div>
        {1}
        {2}
        {3}
    </div>
</section>",
                subheading,
                RenderSearchBar("/latest-post", q),
                searchMeta,
                RenderRowList(posts, true)));
            sb.Append(layout.RenderFooter());
            WriteCached(sb.ToString());
        }
    }
}
