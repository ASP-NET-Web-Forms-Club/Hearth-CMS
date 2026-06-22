using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Web;

namespace System.engine.RH
{
    public static class AdminPosts
    {
        public static void HandleListRequest()
        {
            if (!AdminGuard.RequireLogin()) return;

            // Which view: active (is_deleted=0) or the deleted "trash" filter.
            bool showDeleted = string.Equals(
                (HttpContext.Current.Request.QueryString["filter"] + "").Trim(),
                "deleted", StringComparison.OrdinalIgnoreCase);

            List<obPost> lst = new List<obPost>();
            int activeCount = 0, deletedCount = 0;
            using (var conn = new SQLiteConnection(Config.GetConnString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var s = new SQLiteExpress(cmd);
                    activeCount = s.ExecuteScalar<int>("SELECT COUNT(*) FROM posts WHERE is_deleted=0;");
                    deletedCount = s.ExecuteScalar<int>("SELECT COUNT(*) FROM posts WHERE is_deleted=1;");
                    lst = s.GetObjectList<obPost>(
                        "SELECT * FROM posts WHERE is_deleted=" + (showDeleted ? "1" : "0") +
                        " ORDER BY date_published DESC, date_created DESC;");
                }
            }

            var tpl = new AdminTemplate
            {
                Title = "Posts",
                ActiveItem = "posts",
                PageHeading = "Posts",
                PageHeadingActionsHtml = "<a href='/admin/posts/new' class='btn btn-primary btn-sm'><i class='fa-solid fa-feather-pointed'></i> New post</a>"
            };

            var sb = new StringBuilder();
            sb.Append(tpl.RenderHeader());

            sb.Append(AdminTrashUi.FilterTabs("/admin/posts", showDeleted, activeCount, deletedCount));

            if (lst.Count == 0)
            {
                if (showDeleted)
                {
                    sb.Append(@"
<div class='empty-card empty-card-sm'>
    <i class='fa-solid fa-trash-can empty-icon'></i>
    <h2>Nothing in the trash</h2>
    <p>Deleted posts will appear here and can be restored.</p>
</div>
");
                }
                else
                {
                    sb.Append(@"
<div class='empty-card'>
    <i class='fa-solid fa-pen-nib empty-icon'></i>
    <h2>No posts yet</h2>
    <p>Posts power your blog or news section.</p>
    <a href='/admin/posts/new' class='btn btn-primary'><i class='fa-solid fa-feather-pointed'></i> Write your first post</a>
</div>
");
                }
            }
            else
            {
                sb.Append(AdminTrashUi.BulkBar("post", showDeleted));

                sb.Append(@"
<div class='data-table-wrap'>
<table class='data-table'>
    <thead>
        <tr>
            <th class='col-check'><input type='checkbox' id='checkAll' onclick='toggleAll(this)' /></th>
            <th>Title</th>
            <th>Slug</th>
            <th class='col-narrow'>Status</th>
            <th class='col-narrow'>Published</th>
            <th class='col-actions'></th>
        </tr>
    </thead>
    <tbody>
");
                foreach (var p in lst)
                {
                    string statusBadge = p.IsPublished == 1
                        ? "<span class='badge badge-success'>Published</span>"
                        : "<span class='badge badge-muted'>Draft</span>";

                    string rowActions = showDeleted
                        ? $@"<button type='button' class='icon-btn' onclick='undeletePost({p.Id})' title='Restore'><i class='fa-solid fa-rotate-left'></i></button>
                <button type='button' class='icon-btn icon-btn-danger' onclick='purgePost({p.Id})' title='Permanently delete'><i class='fa-solid fa-trash-can'></i></button>"
                        : $@"<a href='/{HttpUtility.HtmlAttributeEncode(p.Slug)}' target='_blank' class='icon-btn' title='View'><i class='fa-solid fa-arrow-up-right-from-square'></i></a>
                <a href='/admin/posts/edit/{p.Id}' class='icon-btn' title='Edit'><i class='fa-solid fa-pen'></i></a>
                <button type='button' class='icon-btn icon-btn-danger' onclick='deletePost({p.Id})' title='Delete'><i class='fa-solid fa-trash'></i></button>";

                    sb.Append($@"
        <tr>
            <td class='col-check'><input type='checkbox' class='row-check' value='{p.Id}' onclick='syncBulk()' /></td>
            <td><a href='/admin/posts/edit/{p.Id}' class='row-title'>{HttpUtility.HtmlEncode(p.Title)}</a></td>
            <td><code>/{HttpUtility.HtmlEncode(p.Slug)}</code></td>
            <td>{statusBadge}</td>
            <td class='text-muted'>{(p.IsPublished == 1 ? DateDisplay.Format(p.DatePublished) : "-")}</td>
            <td class='col-actions'>
                {rowActions}
            </td>
        </tr>");
                }
                sb.Append(@"
    </tbody>
</table>
</div>
");
                sb.Append(AdminTrashUi.Script("post", "/api/admin/posts"));
            }
            sb.Append(tpl.RenderFooter());
            ApiHelper.WriteHtml(sb.ToString());
            ApiHelper.EndResponse();
        }

        public static void HandleEditRequest(int id)
        {
            if (!AdminGuard.RequireLogin()) return;

            obPost post = new obPost();
            if (id > 0)
            {
                using (var conn = new SQLiteConnection(Config.GetConnString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var s = new SQLiteExpress(cmd);
                        var p = new Dictionary<string, object> { { "@id", id } };
                        var got = s.GetObject<obPost>("SELECT * FROM posts WHERE id=@id LIMIT 1;", p);
                        if (got != null && got.Id > 0) post = got;
                    }
                }
                if (post.Id == 0)
                {
                    AppSession.SetFlash("Post not found");
                    ApiHelper.Redirect("/admin/posts");
                    return;
                }
            }

            var cfg = new ContentEditorConfig
            {
                Id = post.Id,
                EntityLabel = "Post",
                ListUrl = "/admin/posts",
                ApiUrl = "/api/admin/posts",
                ActiveNavItem = "posts",
                PublicUrlPrefix = "/",

                ShowExcerpt = true,
                ShowCoverImage = true,
                ShowCategory = true,
                CategoryOptions = CategoryManager.GetAll(),
                ShowLayoutSelect = true,
                ShowPublishDate = true,
                ShowEditorTypeSelect = true,

                ContentPlaceholder = "<p>Tell your story…</p>",
                ExcerptPlaceholder = "Short summary shown on listings",

                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                ContentFormat = string.IsNullOrEmpty(post.ContentFormat) ? "html" : post.ContentFormat,
                Excerpt = post.Excerpt,
                CoverImage = post.CoverImage,
                CategoryId = post.CategoryId,
                Layout = string.IsNullOrEmpty(post.Layout) ? "split" : post.Layout,
                IsPublished = post.IsPublished,
                DatePublished = post.DatePublished
            };
            ContentEditor.Render(cfg);
        }
    }
}