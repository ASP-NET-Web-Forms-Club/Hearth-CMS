using System.Collections.Generic;
using System.Text;
using System.Web;

namespace System.engine.RH
{
    // ============================================================
    // DocLayout - the ONE universal document renderer.
    //
    // Both "page/post layout 1" (left/right, split) and
    // "page/post layout 2" (top/bottom, stack) are produced by
    // THIS SINGLE method. The markup is identical for both; only
    // the layout class on the root <article> differs. CSS in
    // _base.css does all the rearranging from there.
    //
    //   layout = "split"  -> main content + right sidebar
    //   layout = "stack"  -> full-width content, recent posts below
    //
    // Pages pass ShowAside=false (no "recent posts"); posts pass
    // ShowAside=true. Default layout is decided by the caller
    // (post -> split, page -> stack) until a per-article [layout]
    // column exists.
    // ============================================================

    public static class DocLayout
    {
        // ============================================================
        // RenderTemplated - the external-template path. Picks the active
        // theme's article layout file by the resolved layout:
        //   stack -> article-full-width.html
        //   split -> article-sidebar.html
        // and fills its {{tokens}}. The layout is the real axis (a page or
        // a post can use either); the aside is a slot C# fills with the
        // recent-posts block or leaves empty (block-or-nothing in C#).
        // ============================================================
        public static string RenderTemplated(DocModel m)
        {
            string slug = ThemeManager.GetActiveSlug();
            bool hasAside = m.ShowAside && m.Recent != null && m.Recent.Count > 0;
            string templateName = (m.Layout == "stack")
                ? "article-full-width.html"
                : "article-sidebar.html";

            var model = new TemplateModel();
            model.SetText("article_title", m.Title);
            model.SetRaw("breadcrumbs", BuildBreadcrumbs(m.Breadcrumbs));
            model.SetRaw("published_date", BuildDateMeta(m.PublishedDate));
            model.SetRaw("updated_date", BuildDateUpdate(m.UpdatedDate));
            model.SetRaw("article_author", BuildAuthor(m.Author));
            // Cover image is its own component (components/cover-image.html) so
            // both article layouts share a single definition. Rendered only when
            // there is a cover; pages without one get nothing (the {{cover_image}}
            // token stays empty), so the component needs no "empty" modifier.
            if (!string.IsNullOrEmpty(m.CoverImage))
            {
                var coverModel = new TemplateModel();
                coverModel.SetAttr("article_cover", m.CoverImage);
                model.SetRaw("cover_image", TemplateEngine.Render(slug, "components/cover-image.html", coverModel));
            }
            model.SetRaw("article_content", m.RenderedContentHtml ?? "");
            model.SetRaw("article_aside", hasAside ? BuildAside(m) : "");

            return TemplateEngine.Render(slug, templateName, model);
        }

        static string BuildBreadcrumbs(List<DocCrumb> crumbs)
        {
            if (crumbs == null || crumbs.Count == 0) return "";
            var sb = new StringBuilder();
            sb.Append("<nav class='breadcrumbs' aria-label='Breadcrumb'>");
            for (int i = 0; i < crumbs.Count; i++)
            {
                var c = crumbs[i];
                if (i > 0) sb.Append("<span class='sep'>/</span>");
                if (!string.IsNullOrEmpty(c.Href))
                    sb.Append($"<a href='{HttpUtility.HtmlAttributeEncode(c.Href)}'>{HttpUtility.HtmlEncode(c.Label)}</a>");
                else
                    sb.Append($"<span class='crumb-current'>{HttpUtility.HtmlEncode(c.Label)}</span>");
            }
            sb.Append("</nav>");
            return sb.ToString();
        }

        static string BuildDateMeta(DateTime? date)
        {
            if (!date.HasValue) return "";
            return $"<div class='doc-meta'><i class='fa-regular fa-calendar'></i> {DateDisplay.Format(date.Value)}</div>";
        }

        // Optional "last updated" line. Block-or-nothing: empty when no date.
        static string BuildDateUpdate(DateTime? date)
        {
            if (!date.HasValue) return "";
            return $"<div class='doc-meta doc-meta-update'><i class='fa-regular fa-pen-to-square'></i> Updated {DateDisplay.Format(date.Value)}</div>";
        }

        // Optional author line. Block-or-nothing: empty when no author name.
        static string BuildAuthor(string author)
        {
            if (string.IsNullOrEmpty(author)) return "";
            return $"<div class='doc-meta doc-meta-author'><i class='fa-regular fa-user'></i> {HttpUtility.HtmlEncode(author)}</div>";
        }

        // Recent-posts aside. Like the other repeating pieces, this is built
        // from external component templates (components/article-aside.html wrapper
        // + components/rec-item.html per item); C# only fills data tokens, it emits
        // no layout markup. The cover thumbnail is a bare <img> (or empty) in the
        // {{post_thumb_img}} token, consistent with the list/card thumbnails.
        static string BuildAside(DocModel m)
        {
            string slug = ThemeManager.GetActiveSlug();
            string itemTpl = TemplateEngine.Load(slug, "components/rec-item.html");

            var items = new StringBuilder();
            foreach (var r in m.Recent)
            {
                string thumbImg = string.IsNullOrEmpty(r.ImageUrl)
                    ? ""
                    : $"<img src='{HttpUtility.HtmlAttributeEncode(r.ImageUrl)}' alt='' />";

                var im = new TemplateModel();
                im.SetAttr("post_url", r.Href);
                im.SetRaw("post_thumb_empty", string.IsNullOrEmpty(r.ImageUrl) ? " is-empty" : "");
                im.SetRaw("post_thumb_img", thumbImg);
                im.SetText("post_title", r.Title);
                im.SetText("post_date", DateDisplay.Format(r.Date));
                items.Append(im.Render(itemTpl));
            }

            var wrap = new TemplateModel();
            wrap.SetText("aside_heading", m.AsideHeading);
            wrap.SetRaw("recent_item_list", items.ToString());
            return TemplateEngine.Render(slug, "components/article-aside.html", wrap);
        }
    }
}
