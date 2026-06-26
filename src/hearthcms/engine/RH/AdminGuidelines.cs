using System.Text;
using System.Web;

namespace System.engine.RH
{
    // ============================================================
    // /admin/guidelines - "General Guidelines".
    //
    // A plain documentation page for site operators. Currently it
    // explains how to change the admin login path (Settings -> the
    // config.txt override -> /reset_app), with the live admin slug
    // substituted in so the examples match the current install.
    // ============================================================
    public static class AdminGuidelines
    {
        public static void HandleRequest()
        {
            if (!AdminGuard.RequireLogin()) return;

            var tpl = new AdminTemplate
            {
                Title = "General Guidelines",
                ActiveItem = "guidelines",
                PageHeading = "General Guidelines"
            };

            // Live values so every example matches this install exactly.
            string slug = AdminSlug.Current;
            string slugEnc = HttpUtility.HtmlEncode(slug);
            bool lockedByConfig = AdminSlug.IsLockedByConfig;

            string lockedNote = lockedByConfig
                ? "<p class='form-hint' style='color:var(--warn,#d97706)'><i class='fa-solid fa-lock'></i> Right now the admin path is pinned by <code>/App_Data/config.txt</code>, so the Settings field is read-only. Change it in the file and reload (step 2 and 3 below).</p>"
                : "";

            var sb = new StringBuilder();
            sb.Append(tpl.RenderHeader());

            sb.Append($@"
<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-circle-info'></i> About this page</h2></div>
    <div class='card-body'>
        <p>Operational notes for running this site. Each section is a short how-to for a task that isn't part of day-to-day writing.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-book'></i> Documentation &amp; guides</h2></div>
    <div class='card-body'>
        <ul>
            <li><a href='/admin/themes/docs'>HTML Template Guide</a> &mdash; authoring token-based HTML themes.</li>
            <li><a href='/admin/themes/docs-csharp'>C# Template Guide</a> &mdash; building code-rendered themes by inheriting <code>CsTemplate</code> (and scaffolding an application on the Pageless ASP.NET Web Forms architecture).</li>
            <li><a href='/admin/markdown-docs'>Markdown Documentation</a> &mdash; the supported Markdown syntax and how each construct renders.</li>
        </ul>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-truck-arrow-right'></i> Migrating content in via the API (WordPress &amp; batch import)</h2></div>
    <div class='card-body'>
        <p>The CMS exposes a small HTTP API that a migration tool (or any script/agent) can drive to create posts, upload media, and create categories autonomously &mdash; for example to move a WordPress site over. The recommended flow is: convert each article to <strong>Markdown</strong>, upload its images first so you know their final URLs, rewrite the image links in the Markdown to those URLs, then create the post.</p>

        <h3>Media path format</h3>
        <p>All imported media is stored under a predictable, date-based path:</p>
        <pre><code>/media/{{year}}/{{month}}/{{media_filename}}</code></pre>
        <p>The month is always two digits (e.g. <code>/media/2026/06/photo.jpg</code>). Because the year and month are supplied by you on upload, the tool can compute the final URL of every image <em>before</em> uploading and rewrite the article body up front.</p>

        <h3>Endpoints</h3>
        <table class='data-table'>
            <thead><tr><th>Purpose</th><th>Method &amp; URL</th><th>Body</th></tr></thead>
            <tbody>
                <tr><td>Upload image(s)</td><td><code>POST /api/migration-import</code></td><td><code>multipart/form-data</code></td></tr>
                <tr><td>Create / update category</td><td><code>POST /api/admin/categories</code></td><td><code>multipart/form-data</code> or form-encoded</td></tr>
                <tr><td>Create / update post</td><td><code>POST /api/admin/posts</code></td><td><code>multipart/form-data</code> or form-encoded</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>Every endpoint returns JSON. Success looks like <code>{{ ""success"": true, &hellip; }}</code>; failures return <code>{{ ""success"": false, ""message"": ""&hellip;"" }}</code> with an HTTP 4xx/5xx status.</p>

        <h3>1. Upload images &mdash; <code>/api/migration-import</code></h3>
        <p>Send the file part(s) plus the target year and month. The response gives you the final root-relative <code>url</code> of each file to drop back into your Markdown.</p>
        <table class='data-table'>
            <thead><tr><th>Field</th><th>Notes</th></tr></thead>
            <tbody>
                <tr><td><code>filebytes</code></td><td>The file part. Send several file parts in one request to upload many at once.</td></tr>
                <tr><td><code>year</code></td><td>4-digit year, e.g. <code>2026</code>.</td></tr>
                <tr><td><code>month</code></td><td>1&ndash;2 digit month, e.g. <code>6</code> or <code>06</code> (normalised to two digits).</td></tr>
                <tr><td><code>filename</code></td><td>Optional desired name. For multi-file requests, repeat the field index-aligned with the files; omitted entries fall back to the uploaded part's own name.</td></tr>
                <tr><td><code>build_thumbnail</code></td><td>Optional <code>true</code>/<code>1</code> to queue a background thumbnail.</td></tr>
            </tbody>
        </table>
        <p>Allowed image types: <code>.jpg .jpeg .png .gif .webp .svg .bmp .ico</code>. Names are sanitized, and a collision in the same folder appends <code>-2</code>, <code>-3</code>, &hellip;. Use this for both <strong>feature/cover images</strong> and <strong>in-line article images</strong> &mdash; they all land in the same <code>/media/{{year}}/{{month}}/</code> tree.</p>

        <h3>2. Categories &mdash; <code>/api/admin/categories</code></h3>
        <p>Posts reference a category by numeric id. Before creating posts, you need each category's id. There are two ways to get it:</p>

        <p><strong>List existing categories</strong> &mdash; send <code>action=list</code> to discover ids, names and slugs (useful for mapping WordPress categories and for re-runs):</p>
        <pre><code>{{
  ""success"": true,
  ""data"": {{
    ""count"": 2,
    ""categories"": [
      {{ ""id"": 1, ""name"": ""News"", ""slug"": ""news"", ""description"": """", ""sort_order"": 0, ""post_count"": 12 }},
      {{ ""id"": 2, ""name"": ""Guides"", ""slug"": ""guides"", ""description"": """", ""sort_order"": 0, ""post_count"": 5 }}
    ]
  }}
}}</code></pre>

        <p><strong>Create a category</strong> &mdash; send <code>action=save</code>. Keep the returned id.</p>
        <table class='data-table'>
            <thead><tr><th>Field</th><th>Notes</th></tr></thead>
            <tbody>
                <tr><td><code>action</code></td><td><code>list</code> to read all; <code>save</code> to create/update.</td></tr>
                <tr><td><code>name</code></td><td>Required for <code>save</code>. The display name.</td></tr>
                <tr><td><code>slug</code></td><td>Optional; auto-derived from the name when omitted.</td></tr>
                <tr><td><code>description</code>, <code>cover_image</code>, <code>sort_order</code></td><td>Optional.</td></tr>
            </tbody>
        </table>
        <p class='form-hint'><code>save</code> returns <code>{{ ""success"": true, ""data"": {{ ""id"": &lt;categoryId&gt;, ""slug"": ""&hellip;"" }} }}</code>. Use that <code>id</code> as <code>category_id</code> when creating posts.</p>
        <p class='form-hint'>If a category with the same slug already exists, <code>save</code> does <strong>not</strong> create a duplicate: it returns <code>{{ ""success"": false, ""existing_id"": &lt;id&gt;, ""existing_slug"": ""&hellip;"", ""existing_name"": ""&hellip;"" }}</code>. An importer can simply reuse <code>existing_id</code>, which makes re-running the migration safe.</p>

        <h3>3. Create the post &mdash; <code>/api/admin/posts</code></h3>
        <table class='data-table'>
            <thead><tr><th>Field</th><th>Notes</th></tr></thead>
            <tbody>
                <tr><td><code>action</code></td><td><code>save</code></td></tr>
                <tr><td><code>title</code></td><td>Required.</td></tr>
                <tr><td><code>content</code></td><td>The article body (your converted Markdown, with image URLs already rewritten to <code>/media/&hellip;</code>).</td></tr>
                <tr><td><code>content_format</code></td><td><code>markdown</code> to store and render the body as Markdown (anything else is treated as <code>html</code>).</td></tr>
                <tr><td><code>slug</code></td><td>Optional; auto-derived from the title when omitted. Must be unique.</td></tr>
                <tr><td><code>excerpt</code></td><td>Optional plain-text summary (Markdown/HTML is stripped).</td></tr>
                <tr><td><code>cover_image</code></td><td>The feature image URL, e.g. the <code>/media/&hellip;</code> URL returned in step 1.</td></tr>
                <tr><td><code>category_id</code></td><td>Numeric category id (<code>0</code> = uncategorized). An id that doesn't resolve is treated as uncategorized.</td></tr>
                <tr><td><code>is_published</code></td><td><code>1</code> to publish, <code>0</code> for draft.</td></tr>
                <tr><td><code>date_published</code></td><td>Optional explicit publish date (preserve the original WordPress date here). Parsed as a date/time, e.g. <code>2024-03-15</code> or <code>2024-03-15 09:30:00</code>. When omitted on a published post, the current time is stamped.</td></tr>
                <tr><td><code>layout</code></td><td>Optional: <code>split</code> (default) or <code>stack</code>.</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>To <em>update</em> an existing post instead of creating one, include its <code>id</code>.</p>

        <h3 style='color:var(--warn,#d97706)'><i class='fa-solid fa-triangle-exclamation'></i> Authentication &mdash; required for the import to work</h3>
        <p>Every API endpoint above is protected exactly like the rest of the admin: the request must be <strong>authenticated</strong>. A migration tool posting autonomously has no login session, so by default these calls are rejected with <code>401 Not signed in</code>. There are three ways to let an automated importer through:</p>
        <ul>
            <li><strong>API token (recommended for unattended / agent-driven posting).</strong> In <a href='/admin/settings'>Settings</a> &rarr; <strong>API access token</strong>, generate a token and copy it. Send it with each API request as the <code>X-Api-Token</code> header, or as an <code>api_token</code> query-string or form field. The CMS checks it alongside the normal login, so a script, an unattended pipeline, or an AI agent submitting via an MCP/tool call can post <strong>without Dev Mode and without a human login</strong>.
                <pre><code># PowerShell example: post an article with the token in a header
$headers = @{{ ""X-Api-Token"" = ""YOUR_TOKEN_HERE"" }}
$form = @{{
    action         = ""save""
    title          = ""My migrated article""
    content        = ""# Hello`n`nBody in **Markdown**...""
    content_format = ""markdown""
    is_published   = ""1""
    date_published = ""2024-03-15 09:30:00""
    cover_image    = ""/media/2024/03/feature.jpg""
}}
Invoke-RestMethod -Uri ""https://your-site/api/admin/posts"" -Method Post -Headers $headers -Form $form</code></pre>
                <p class='form-hint' style='color:var(--warn,#d97706);margin:6px 0 0'><i class='fa-solid fa-triangle-exclamation'></i> Treat the token like a password: it grants full admin-level create/modify access. Prefer the header over the query string (query strings can end up in server logs). Always serve the API over HTTPS, and regenerate the token if it may have leaked &mdash; regenerating invalidates the old one immediately.</p>
            </li>
            <li><strong>Enable Dev Mode</strong> (simplest for a local, one-off migration). In <a href='/admin/settings'>Settings</a> &rarr; <strong>Dev / Testing</strong>, turn on <em>Dev Mode</em>. While it's on, the request pipeline auto-logs the first admin user, so API calls succeed <strong>without login credentials</strong>. Run your import, then <strong>turn Dev Mode back off</strong>.
                <p class='form-hint' style='color:var(--warn,#d97706);margin:6px 0 0'><i class='fa-solid fa-triangle-exclamation'></i> Never leave Dev Mode enabled on a public/production site &mdash; with it on, anyone who reaches an admin or API URL is treated as the admin, with no password. Do the migration locally, or take the site offline for the duration.</p>
            </li>
            <li><strong>Authenticate the request yourself.</strong> Your tool can instead carry a valid admin session &mdash; for example by performing the normal login first and reusing the session cookie on subsequent API calls.</li>
        </ul>

        <h3>Building the client (MCP tool / PowerShell)</h3>
        <p>The CMS side is now complete: predictable endpoints, a media path it understands, and token auth for unattended callers. The remaining work is on <em>your</em> side &mdash; a small client that speaks this protocol: it converts each article to Markdown, uploads the images (<code>/api/migration-import</code>), rewrites the in-line and feature image URLs to the returned <code>/media/&hellip;</code> paths, then creates the post (<code>/api/admin/posts</code>), sending the API token on every request. That client can be a custom MCP server tool, a PowerShell script, or any HTTP client.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-user-lock'></i> Changing the admin login path</h2></div>
    <div class='card-body'>
        <p>The admin panel lives under a single URL segment &mdash; by default <code>/admin</code>, currently <code>/{slugEnc}</code>. That segment is also the <strong>login screen</strong>: visit it while signed out and you'll be asked to sign in. Renaming it to something less obvious (for example <code>/backend</code> or <code>/controlpanel</code>) keeps the panel hidden from the public; once you're signed in, everything works normally.</p>
        <p>There are two ways to set it, and a way to reload after editing the file.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-1'></i> First choice &mdash; change it in Settings</h2></div>
    <div class='card-body'>
        <p>The normal way. Go to <a href='/admin/settings'>Settings</a> &rarr; <strong>Admin URL</strong>, type the new path segment, and save. The panel moves immediately and you'll be redirected to the new address.</p>
        <ul>
            <li>Use letters, numbers, hyphens and underscores only.</li>
            <li>Reserved words (<code>logout</code>, <code>api</code>, <code>category</code>, <code>reset_app</code>, <code>home</code>, &hellip;) are rejected.</li>
            <li><strong>Bookmark the new URL</strong> as soon as you save &mdash; the old one stops working.</li>
        </ul>
        <p class='form-hint'>This value is stored in the database. It applies unless a <code>config.txt</code> override is present (next section), which always wins.</p>
        {lockedNote}
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-2'></i> Override &mdash; <code>config.txt</code></h2></div>
    <div class='card-body'>
        <p>A file-based override that <strong>always wins over the Settings value</strong>. This is the safety hatch: if you ever forget your custom admin path and lock yourself out, you can set it here without touching the database or the code.</p>
        <p>Create a file at:</p>
        <pre><code>/App_Data/config.txt</code></pre>
        <p>and add a single line naming the path segment:</p>
        <pre><code>admin_url=backend</code></pre>
        <p>That puts the panel at <code>/backend</code>. To put it back to the default, use:</p>
        <pre><code>admin_url=admin</code></pre>
        <ul>
            <li>Lines starting with <code>#</code> or <code>;</code> are comments. The format is <code>key=value</code>.</li>
            <li>While this line is present, the <strong>Admin URL field in Settings becomes read-only</strong> &mdash; the file is in charge. Remove the line (or the file) to hand control back to Settings.</li>
            <li>The same character and reserved-word rules apply; an invalid value is ignored and the system falls back to Settings, then to <code>admin</code>.</li>
        </ul>
        <p class='form-hint'>There is a ready-made template to copy from: <code>/App_Data/config.txt.example</code>.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-3'></i> Loading <code>config.txt</code> &mdash; <code>/reset_app</code></h2></div>
    <div class='card-body'>
        <p>The file is read <strong>once</strong>, so editing it doesn't take effect on its own. There are two ways it gets loaded:</p>
        <ul>
            <li><strong>Automatically</strong> when the web app (or the server) restarts &mdash; the file is read on start-up.</li>
            <li><strong>On demand</strong>, without a restart, by visiting <a href='/reset_app'><code>/reset_app</code></a> in your browser. It re-reads <code>config.txt</code> and reports where the admin panel now lives.</li>
        </ul>
        <p>So the recovery flow for a forgotten admin path is:</p>
        <ol>
            <li>Create or edit <code>/App_Data/config.txt</code> with <code>admin_url=admin</code> (or any path you like).</li>
            <li>Open <code>/reset_app</code> &mdash; or just restart the app.</li>
            <li>Go to the path it reports and sign in.</li>
        </ol>
        <p class='form-hint'>Tip: <code>/reset_app</code> always works regardless of the current admin path, so it's safe to rely on even when you're locked out of the panel.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-list-ol'></i> Order of precedence</h2></div>
    <div class='card-body'>
        <p>When the site decides where the admin panel lives, it checks, in order:</p>
        <table class='data-table'>
            <thead><tr><th>#</th><th>Source</th><th>Wins when&hellip;</th></tr></thead>
            <tbody>
                <tr><td>1</td><td><code>/App_Data/config.txt</code> &rarr; <code>admin_url=</code></td><td>the file exists and the value is valid</td></tr>
                <tr><td>2</td><td>Settings &rarr; Admin URL (database)</td><td>no file override, and a value was saved</td></tr>
                <tr><td>3</td><td>Built-in default <code>admin</code></td><td>neither of the above is set</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>Current admin path on this install: <code>/{slugEnc}</code>{(lockedByConfig ? " (pinned by config.txt)" : "")}.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-key'></i> Resetting a forgotten admin login</h2></div>
    <div class='card-body'>
        <p>If you forget the admin <strong>username or password</strong>, reset them with a one-shot file. This is separate from <code>config.txt</code> and does not leave any credentials lying around.</p>
        <ol>
            <li>Create a file at <code>/App_Data/reset_admin.txt</code> with two lines:
                <pre><code>admin_username=youradmin
admin_password=your-new-password</code></pre>
            </li>
            <li>Restart the app, or visit <a href='/reset_app'><code>/reset_app</code></a>.</li>
            <li>The admin login is reset to those values, and the file is <strong>automatically consumed</strong> &mdash; deleted if possible, otherwise blanked &mdash; so the password never lingers on disk.</li>
            <li>Sign in with the new credentials, then change the password again from the admin panel if you like.</li>
        </ol>
        <ul>
            <li>Username must be at least 3 characters; password at least 6.</li>
            <li>The reset is only applied <em>after</em> the file has been safely removed or blanked &mdash; so the same file can never be replayed.</li>
            <li>Unlike the old approach, nothing is stored as a standing password in <code>config.txt</code>; the new password is hashed into the database like any other.</li>
        </ul>
        <p class='form-hint'>There is a ready-made template to copy from: <code>/App_Data/reset_admin.txt.example</code>.</p>
    </div>
</div>
");

            sb.Append(tpl.RenderFooter());
            ApiHelper.WriteHtml(sb.ToString());
            ApiHelper.EndResponse();
        }
    }
}
