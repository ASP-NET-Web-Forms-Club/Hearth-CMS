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
            <li>Reserved words (<code>blog</code>, <code>logout</code>, <code>api</code>, <code>category</code>, <code>reset_app</code>, <code>home</code>, &hellip;) are rejected.</li>
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
