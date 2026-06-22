using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Web;

namespace System.engine.RH
{
    public static class AdminPages
    {
        public static void HandleListRequest()
        {
            if (!AdminGuard.RequireLogin()) return;

            // Which view: active (is_deleted=0) or the deleted "trash" filter.
            bool showDeleted = string.Equals(
                (HttpContext.Current.Request.QueryString["filter"] + "").Trim(),
                "deleted", StringComparison.OrdinalIgnoreCase);

            List<obPage> lst = new List<obPage>();
            int activeCount = 0, deletedCount = 0;
            using (var conn = new SQLiteConnection(Config.GetConnString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var s = new SQLiteExpress(cmd);
                    activeCount = s.ExecuteScalar<int>("SELECT COUNT(*) FROM pages WHERE is_deleted=0;");
                    deletedCount = s.ExecuteScalar<int>("SELECT COUNT(*) FROM pages WHERE is_deleted=1;");
                    lst = s.GetObjectList<obPage>(
                        "SELECT * FROM pages WHERE is_deleted=" + (showDeleted ? "1" : "0") +
                        " ORDER BY sort_order, title;");
                }
            }

            var tpl = new AdminTemplate
            {
                Title = "Pages",
                ActiveItem = "pages",
                PageHeading = "Pages",
                PageHeadingActionsHtml = "<a href='/admin/pages/new' class='btn btn-primary btn-sm'><i class='fa-solid fa-plus'></i> New page</a>"
            };

            var sb = new StringBuilder();
            sb.Append(tpl.RenderHeader());

            sb.Append(AdminTrashUi.FilterTabs("/admin/pages", showDeleted, activeCount, deletedCount));

            if (lst.Count == 0)
            {
                if (showDeleted)
                {
                    sb.Append(@"
<div class='empty-card empty-card-sm'>
    <i class='fa-solid fa-trash-can empty-icon'></i>
    <h2>Nothing in the trash</h2>
    <p>Deleted pages will appear here and can be restored.</p>
</div>
");
                }
                else
                {
                    sb.Append(@"
<div class='empty-card'>
    <i class='fa-solid fa-file-lines empty-icon'></i>
    <h2>No pages yet</h2>
    <p>Pages are perfect for static content like About and Contact.</p>
    <a href='/admin/pages/new' class='btn btn-primary'><i class='fa-solid fa-plus'></i> Create your first page</a>
</div>
");
                }
            }
            else
            {
                sb.Append(AdminTrashUi.BulkBar("page", showDeleted));

                sb.Append(@"
<div class='data-table-wrap'>
<table class='data-table'>
    <thead>
        <tr>
            <th class='col-check'><input type='checkbox' id='checkAll' onclick='toggleAll(this)' /></th>
            <th>Title</th>
            <th>Slug</th>
            <th class='col-narrow'>Nav</th>
            <th class='col-narrow'>Status</th>
            <th class='col-narrow'>Modified</th>
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
                    string navIcon = p.ShowInNav == 1
                        ? "<i class='fa-solid fa-check text-success' title='In nav'></i>"
                        : "<span class='text-muted'>-</span>";

                    string rowActions = showDeleted
                        ? $@"<button type='button' class='icon-btn' onclick='undeletePage({p.Id})' title='Restore'><i class='fa-solid fa-rotate-left'></i></button>
                <button type='button' class='icon-btn icon-btn-danger' onclick='purgePage({p.Id})' title='Permanently delete'><i class='fa-solid fa-trash-can'></i></button>"
                        : $@"<a href='/{HttpUtility.HtmlAttributeEncode(p.Slug)}' target='_blank' class='icon-btn' title='View'><i class='fa-solid fa-arrow-up-right-from-square'></i></a>
                <a href='/admin/pages/edit/{p.Id}' class='icon-btn' title='Edit'><i class='fa-solid fa-pen'></i></a>
                <button type='button' class='icon-btn icon-btn-danger' onclick='deletePage({p.Id})' title='Delete'><i class='fa-solid fa-trash'></i></button>";

                    sb.Append($@"
        <tr>
            <td class='col-check'><input type='checkbox' class='row-check' value='{p.Id}' onclick='syncBulk()' /></td>
            <td><a href='/admin/pages/edit/{p.Id}' class='row-title'>{HttpUtility.HtmlEncode(p.Title)}</a></td>
            <td><code>/{HttpUtility.HtmlEncode(p.Slug)}</code></td>
            <td>{navIcon}</td>
            <td>{statusBadge}</td>
            <td class='text-muted'>{DateDisplay.Format(p.DateModified)}</td>
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
                sb.Append(AdminTrashUi.Script("page", "/api/admin/pages"));
            }
            sb.Append(tpl.RenderFooter());
            ApiHelper.WriteHtml(sb.ToString());
            ApiHelper.EndResponse();
        }

        public static void HandleEditRequest(int id)
        {
            if (!AdminGuard.RequireLogin()) return;

            obPage page = new obPage();
            if (id > 0)
            {
                using (var conn = new SQLiteConnection(Config.GetConnString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var s = new SQLiteExpress(cmd);
                        var p = new Dictionary<string, object> { { "@id", id } };
                        var got = s.GetObject<obPage>("SELECT * FROM pages WHERE id=@id LIMIT 1;", p);
                        if (got != null && got.Id > 0) page = got;
                    }
                }
                if (page.Id == 0)
                {
                    AppSession.SetFlash("Page not found");
                    ApiHelper.Redirect("/admin/pages");
                    return;
                }
            }

            var cfg = new ContentEditorConfig
            {
                Id = page.Id,
                EntityLabel = "Page",
                ListUrl = "/admin/pages",
                ApiUrl = "/api/admin/pages",
                ActiveNavItem = "pages",
                PublicUrlPrefix = "/",

                ShowExcerpt = true,
                ShowLayoutSelect = true,
                ShowInNavToggle = true,
                ShowSortOrder = true,
                ShowPublishDate = true,
                ShowEditorTypeSelect = true,

                ContentPlaceholder = "<p>Write your page content here…</p>",

                Title = page.Title,
                Slug = page.Slug,
                Content = page.Content,
                ContentFormat = string.IsNullOrEmpty(page.ContentFormat) ? "html" : page.ContentFormat,
                Excerpt = page.Excerpt,
                Layout = string.IsNullOrEmpty(page.Layout) ? "stack" : page.Layout,
                IsPublished = page.IsPublished,
                ShowInNav = page.ShowInNav,
                SortOrder = page.SortOrder,
                DatePublished = page.DatePublished
            };
            ContentEditor.Render(cfg);
        }
    }
}
