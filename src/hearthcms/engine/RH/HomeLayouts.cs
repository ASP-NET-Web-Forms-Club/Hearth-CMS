using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Web;

namespace System.engine.RH
{
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

    internal static class HomeLayoutShared
    {
        public static int HomeCount()
        {
            int n = Settings.HomePostCount;
            return n > 0 ? n : 6;
        }

        // Per-template post-count setting, clamped to a sane 1..50 range.
        // Each public listing template reads its own key (home_post_count,
        // latest_post_count, categories_post_count, category_post_count,
        // article_sidebar_post_count) so admins can tune them independently.
        public static int CountFor(string key, int fallback = 6)
        {
            int n;
            if (!int.TryParse(Db.GetSetting(key, fallback.ToString()), out n)) n = fallback;
            if (n < 1) n = 1;
            if (n > 50) n = 50;
            return n;
        }

        // ----- Queries -----

        public static List<obPost> Latest(SQLiteExpress s, int limit)
        {
            var p = new Dictionary<string, object> { { "@lim", limit } };
            return s.GetObjectList<obPost>(
                "SELECT * FROM posts WHERE is_published=1 AND is_deleted=0 ORDER BY date_published DESC LIMIT @lim;", p);
        }

        public static List<obPost> LatestInCategory(SQLiteExpress s, int categoryId, int limit)
        {
            var p = new Dictionary<string, object> { { "@cat", categoryId }, { "@lim", limit } };
            return s.GetObjectList<obPost>(
                "SELECT * FROM posts WHERE is_published=1 AND is_deleted=0 AND category_id=@cat ORDER BY date_published DESC LIMIT @lim;", p);
        }

        // Multi-key relevance search. Sort priority:
        //   (1) title  (2) excerpt  (3) content  (4) date_published
        // Optional categoryId (>0) scopes the search to one category. Category is
        // no longer free text, so it is not part of the LIKE match; scoping is an
        // exact id filter instead.
        public static List<obPost> Search(SQLiteExpress s, string q, int categoryId, int limit)
        {
            string like = "%" + q + "%";
            var p = new Dictionary<string, object> { { "@q", like }, { "@lim", limit } };

            string scope = "";
            if (categoryId > 0)
            {
                scope = " AND category_id=@cat";
                p["@cat"] = categoryId;
            }

            string sql =
                "SELECT * FROM posts WHERE is_published=1 AND is_deleted=0" + scope + " AND (" +
                "  title LIKE @q OR excerpt LIKE @q OR content LIKE @q" +
                ") ORDER BY " +
                "  CASE WHEN title    LIKE @q THEN 0 ELSE 1 END, " +
                "  CASE WHEN excerpt  LIKE @q THEN 0 ELSE 1 END, " +
                "  CASE WHEN content  LIKE @q THEN 0 ELSE 1 END, " +
                "  date_published DESC LIMIT @lim;";
            return s.GetObjectList<obPost>(sql, p);
        }

        // The managed categories that currently have at least one published,
        // non-deleted post, in the categories table's display order. Returned as
        // full obCategory rows so callers have id, name and slug together.
        public static List<obCategory> DistinctCategories(SQLiteExpress s)
        {
            var lst = new List<obCategory>();
            foreach (var c in CategoryManager.GetAll())
            {
                if (c == null || c.Id <= 0) continue;
                var p = new Dictionary<string, object> { { "@cat", c.Id } };
                int n = s.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM posts WHERE is_published=1 AND is_deleted=0 AND category_id=@cat;", p);
                if (n > 0) lst.Add(c);
            }
            return lst;
        }

        // ----- Rendering -----

        static string Excerpt(obPost p, int n)
        {
            return string.IsNullOrEmpty(p.Excerpt) ? HomePage.ToPlainText(p.Content, n) : p.Excerpt;
        }

        // Card thumbnails use the resized mirror when available (falls back to
        // the original for legacy/hot-linked images). Single post/page cover
        // images intentionally keep the full-size image elsewhere.
        static string BgStyle(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            string display = ImageThumb.DisplayUrl(url);
            return $" style='background-image:url(&quot;{HttpUtility.HtmlAttributeEncode(display)}&quot;)'";
        }

        // Card thumbnail as an <img> element (sized to cover its wrapper via CSS),
        // or empty when there is no cover image. Uses the resized mirror like
        // BgStyle. Block-or-nothing: the wrapping div carries the is-empty class.
        static string ThumbImg(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            string display = ImageThumb.DisplayUrl(url);
            return $"<img src='{HttpUtility.HtmlAttributeEncode(display)}' alt='' />";
        }

        // Search box (GET). actionPath is the route it submits back to.
        public static string SearchBar(string actionPath, string q)
        {
            return $@"
<form class='search-bar' method='get' action='{HttpUtility.HtmlAttributeEncode(actionPath)}'>
    <i class='fa-solid fa-magnifying-glass search-ico'></i>
    <input class='search-input' type='search' name='q' value='{HttpUtility.HtmlAttributeEncode(q)}' placeholder='Search posts…' aria-label='Search posts' />
    <button type='submit' class='btn btn-primary btn-sm'>Search</button>
</form>";
        }

        // Layout 1 : flat row list. [thumb | title / excerpt / date / category]
        // catMap resolves a post's category_id to its display name for the tag;
        // pass null (or showCategory=false) to omit the category tag entirely.
        public static string RowList(List<obPost> posts, bool showCategory, Dictionary<int, obCategory> catMap = null)
        {
            if (posts == null || posts.Count == 0)
                return "<p class='empty-state'>No posts found.</p>";

            var sb = new StringBuilder();
            sb.Append("<div class='row-list'>");
            foreach (var p in posts)
            {
                string emptyCls = string.IsNullOrEmpty(p.CoverImage) ? " is-empty" : "";
                sb.Append($@"
    <article class='row-post'>
        <a class='row-link' href='/{HttpUtility.HtmlAttributeEncode(p.Slug)}'>
            <span class='row-thumb{emptyCls}'>{ThumbImg(p.CoverImage)}</span>
            <span class='row-body'>
                <span class='row-title'>{HttpUtility.HtmlEncode(p.Title)}</span>
                <span class='row-excerpt'>{HttpUtility.HtmlEncode(Excerpt(p, 180))}</span>
                <span class='row-meta'>
                    <span class='row-date'><i class='fa-regular fa-calendar'></i> {DateDisplay.Format(p.DatePublished)}</span>");
                string catName = CatName(p, catMap);
                if (showCategory && catName.Length > 0)
                {
                    sb.Append($"\n                    <span class='row-cat'><i class='fa-solid fa-tag'></i> {HttpUtility.HtmlEncode(catName)}</span>");
                }
                sb.Append(@"
                </span>
            </span>
        </a>
    </article>");
            }
            sb.Append("\n</div>");
            return sb.ToString();
        }

        // Resolve a post's category display name via the supplied map (preferred,
        // no query) or a direct lookup as a fallback. Returns "" when uncategorized.
        static string CatName(obPost p, Dictionary<int, obCategory> catMap)
        {
            if (p == null || p.CategoryId <= 0) return "";
            obCategory c;
            if (catMap != null && catMap.TryGetValue(p.CategoryId, out c)) return c == null ? "" : c.Name;
            return CategoryManager.NameById(p.CategoryId);
        }

        // Layout 2 : one category section - 3 equal columns.
        // col1 = featured (latest) post; col2 & col3 = compact lists.
        public static string CategorySection(obCategory cat, List<obPost> posts)
        {
            if (posts == null || posts.Count == 0 || cat == null) return "";

            string catSlug = cat.Slug;
            var feature = posts[0];

            // Split the remaining posts across the two compact columns.
            var rest = posts.GetRange(1, posts.Count - 1);
            int mid = (rest.Count + 1) / 2;
            var colA = rest.GetRange(0, System.Math.Min(mid, rest.Count));
            var colB = rest.Count > mid ? rest.GetRange(mid, rest.Count - mid) : new List<obPost>();

            string featEmpty = string.IsNullOrEmpty(feature.CoverImage) ? " is-empty" : "";

            var sb = new StringBuilder();
            sb.Append($@"
<section class='cat-section'>
    <div class='container'>
        <div class='cat-head'>
            <h2>{HttpUtility.HtmlEncode(cat.Name)}</h2>
            <a class='cat-all' href='/category/{HttpUtility.HtmlAttributeEncode(catSlug)}'>View all <i class='fa-solid fa-arrow-right'></i></a>
        </div>
        <div class='cat-grid'>
            <article class='cat-feature'>
                <a class='cat-feature-link' href='/{HttpUtility.HtmlAttributeEncode(feature.Slug)}'>
                    <span class='cat-feature-img{featEmpty}'>{ThumbImg(feature.CoverImage)}</span>
                    <h3 class='cat-feature-title'>{HttpUtility.HtmlEncode(feature.Title)}</h3>
                    <p class='cat-feature-excerpt'>{HttpUtility.HtmlEncode(Excerpt(feature, 200))}</p>
                    <span class='cat-feature-meta'><i class='fa-regular fa-calendar'></i> {DateDisplay.Format(feature.DatePublished)}</span>
                </a>
            </article>
            {MiniList(colA)}
            {MiniList(colB)}
        </div>
    </div>
</section>");
            return sb.ToString();
        }

        static string MiniList(List<obPost> posts)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='cat-mini'>");
            foreach (var p in posts)
            {
                string emptyCls = string.IsNullOrEmpty(p.CoverImage) ? " is-empty" : "";
                sb.Append($@"
                <a class='mini-link' href='/{HttpUtility.HtmlAttributeEncode(p.Slug)}'>
                    <span class='mini-thumb{emptyCls}'>{ThumbImg(p.CoverImage)}</span>
                    <span class='mini-body'>
                        <span class='mini-title'>{HttpUtility.HtmlEncode(p.Title)}</span>
                        <span class='mini-date'>{DateDisplay.Format(p.DatePublished)}</span>
                    </span>
                </a>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        // ============================================================
        // Templated variants - same markup as above, but assembled from
        // the active theme's external components. The page-shell templates
        // (latest-post.html, category.html, categories-latest-post.html)
        // own the chrome; these fill the dynamic item lists.
        // ============================================================

        // Layout 1 row list, built from components/row-post.html.
        public static string RowListTemplated(string themeSlug, List<obPost> posts, bool showCategory, Dictionary<int, obCategory> catMap = null)
        {
            if (posts == null || posts.Count == 0)
                return "<p class='empty-state'>No posts found.</p>";

            string itemTpl = TemplateEngine.Load(themeSlug, "components/row-post.html");
            var sb = new StringBuilder();
            sb.Append("<div class='row-list'>");
            foreach (var p in posts)
            {
                var m = new TemplateModel();
                m.SetAttr("post_url", "/" + p.Slug);
                m.SetRaw("post_thumb_empty", string.IsNullOrEmpty(p.CoverImage) ? " is-empty" : "");
                m.SetRaw("post_thumb_img", ThumbImg(p.CoverImage));
                m.SetText("post_title", p.Title);
                m.SetText("post_excerpt", Excerpt(p, 180));
                m.SetText("post_date", DateDisplay.Format(p.DatePublished));
                string catName = CatName(p, catMap);
                m.SetRaw("post_category",
                    (showCategory && catName.Length > 0)
                        ? "<span class='row-cat'><i class='fa-solid fa-tag'></i> " + HttpUtility.HtmlEncode(catName) + "</span>"
                        : "");
                sb.Append(m.Render(itemTpl));
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        // Layout 2 category section, built from components/category-section.html.
        public static string CategorySectionTemplated(string themeSlug, obCategory cat, List<obPost> posts)
        {
            if (posts == null || posts.Count == 0 || cat == null) return "";

            string catSlug = cat.Slug;
            var feature = posts[0];
            var rest = posts.GetRange(1, posts.Count - 1);
            int mid = (rest.Count + 1) / 2;
            var colA = rest.GetRange(0, System.Math.Min(mid, rest.Count));
            var colB = rest.Count > mid ? rest.GetRange(mid, rest.Count - mid) : new List<obPost>();

            var m = new TemplateModel();
            m.SetText("category_title", cat.Name);
            m.SetAttr("category_url", "/category/" + catSlug);
            m.SetAttr("feature_url", "/" + feature.Slug);
            m.SetRaw("feature_thumb_empty", string.IsNullOrEmpty(feature.CoverImage) ? " is-empty" : "");
            m.SetRaw("feature_thumb_img", ThumbImg(feature.CoverImage));
            m.SetText("feature_title", feature.Title);
            m.SetText("feature_excerpt", Excerpt(feature, 200));
            m.SetText("feature_date", DateDisplay.Format(feature.DatePublished));
            m.SetRaw("column_1_list", MiniItemsTemplated(themeSlug, colA));
            m.SetRaw("column_2_list", MiniItemsTemplated(themeSlug, colB));
            return TemplateEngine.Render(themeSlug, "components/category-section.html", m);
        }

        // Mini-column items only (the .cat-mini wrapper lives in the section template).
        static string MiniItemsTemplated(string themeSlug, List<obPost> posts)
        {
            string itemTpl = TemplateEngine.Load(themeSlug, "components/cat-mini-item.html");
            var sb = new StringBuilder();
            foreach (var p in posts)
            {
                var m = new TemplateModel();
                m.SetAttr("post_url", "/" + p.Slug);
                m.SetRaw("post_thumb_empty", string.IsNullOrEmpty(p.CoverImage) ? " is-empty" : "");
                m.SetRaw("post_thumb_img", ThumbImg(p.CoverImage));
                m.SetText("post_title", p.Title);
                m.SetText("post_date", DateDisplay.Format(p.DatePublished));
                sb.Append(m.Render(itemTpl));
            }
            return sb.ToString();
        }
    }
}
