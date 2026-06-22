using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.engine;
using System.engine.RH;
using System.Linq;
using System.Text;
using System.Web;

// ============================================================
// Phase 3 - Home layouts & category page
//
//   /latest-post            -> LatestPostPage     (layout 1: flat rows)
//   /categories-latest-post -> CategoriesLatestPostPage (layout 2)
//   /category/{slug}         -> CategoryPage       (layout 1, scoped)
//
// All three share the helpers in HomeLayoutShared below.
// Search is a plain GET form (?q=...). Because query strings
// bypass the page cache, search results always render fresh.
// ============================================================

namespace System.engine.RH
{
    // ===== /latest-post : layout 1 (flat list) + global search =====
    public static class LatestPostPage
    {
        public static void HandleRequest()
        {
            string q = (HttpContext.Current.Request.QueryString["q"] + "").Trim();
            int limit = HomeLayoutShared.CountFor("latest_post_count");

            List<obPost> posts;
            try
            {
                using (var conn = new SQLiteConnection(Config.GetConnString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var s = new SQLiteExpress(cmd);
                        posts = string.IsNullOrEmpty(q)
                            ? HomeLayoutShared.Latest(s, limit)
                            : HomeLayoutShared.Search(s, q, 0, limit);
                    }
                }
            }
            catch { posts = new List<obPost>(); }

            var pt = new PublicTemplate { Title = "Latest posts", BodyClass = "page-latest" };
            string themeSlug = ThemeManager.GetActiveSlug();

            var m = new TemplateModel();
            m.SetText("page_heading", "Latest posts");
            m.SetRaw("page_subheading", string.IsNullOrEmpty(q)
                ? "<p class='list-sub'>Fresh writing, newest first.</p>" : "");
            m.SetRaw("search_bar", HomeLayoutShared.SearchBar("/latest-post", q));
            m.SetRaw("search_meta", string.IsNullOrEmpty(q)
                ? ""
                : $"<p class='search-meta'>{posts.Count} result(s) for “{HttpUtility.HtmlEncode(q)}” · <a href='/latest-post'>Clear</a></p>");
            m.SetRaw("row_list", HomeLayoutShared.RowListTemplated(themeSlug, posts, true, CategoryManager.GetMap()));

            string body = TemplateEngine.Render(themeSlug, "latest-post.html", m);
            PublicPageCache.WriteAndCache(pt.RenderPage(body));
        }
    }
}