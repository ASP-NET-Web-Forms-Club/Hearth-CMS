using System.Text;

namespace System.engine.RH
{
    // ============================================================
    // /admin/themes/docs-csharp - the C# (code-rendered) theme guide.
    //
    // Sister page to AdminThemeDocs (the HTML/token guide). Explains
    // building a theme by inheriting CsTemplate, the data API available
    // to theme code, direct SQLite access, registering custom routes in
    // Global.asax, and how Hearth doubles as a scaffold for a Pageless
    // ASP.NET Web Forms application.
    //
    // The body is a plain (non-interpolated) verbatim string so the literal
    // C# braces and {{token}} examples render as-is.
    // ============================================================
    public static class AdminThemeDocsCSharp
    {
        public static void HandleRequest()
        {
            if (!AdminGuard.RequireLogin()) return;

            var tpl = new AdminTemplate
            {
                Title = "C# Template guide",
                ActiveItem = "themes",
                PageHeading = "C# Template authoring guide",
                PageHeadingActionsHtml = "<a href='/admin/themes' class='btn btn-ghost btn-sm'><i class='fa-solid fa-arrow-left'></i> Back to Themes</a>"
            };

            var sb = new StringBuilder();
            sb.Append(tpl.RenderHeader());
            sb.Append(Body);
            sb.Append(tpl.RenderFooter());
            ApiHelper.WriteHtml(sb.ToString());
            ApiHelper.EndResponse();
        }

        const string Body = @"
<div class='doc-guide-nav' style='display:flex;gap:8px;margin-bottom:18px;flex-wrap:wrap'>
    <a href='/admin/themes/docs' class='btn btn-sm btn-ghost'>HTML Template Guide</a>
    <a href='/admin/themes/docs-csharp' class='btn btn-sm btn-primary'>C# Template Guide</a>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-code'></i> What is a C# template?</h2></div>
    <div class='card-body'>
        <p>A <strong>C# theme</strong> renders each page imperatively in code instead of filling <code>{{token}}</code> placeholders in HTML files. Where an HTML theme is constrained to a fixed data shape, a C# theme can do <em>anything C# can do</em> &mdash; arbitrary layout, loops and conditionals, custom queries, third-party API calls, even its own endpoints.</p>
        <p>A C# theme is simply <strong>a class that inherits <code>CsTemplate</code></strong>, compiled into the application. There is no flag or attribute to set &mdash; deriving from the base class is what marks the class as a theme. The registry scans the loaded assemblies, finds your class, and activates it when its <code>Slug</code> matches the active theme.</p>
        <p class='form-hint'>The shipped <strong>Broadsheet (C#)</strong> theme (<code>/engine/CsTemplate/Themes/Broadsheet/</code>) is a complete, working reference &mdash; copy it as your starting point.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-rocket'></i> More than a theme &mdash; an application scaffold</h2></div>
    <div class='card-body'>
        <p>A C# template is not limited to &ldquo;skinning a blog.&rdquo; Because you have the full language, the database, and the request pipeline at your disposal, Hearth becomes a <strong>fast scaffold for building a small application from the ground up</strong> &mdash; SQLite already wired in, sessions and admin authentication handled, caching in place, and a content model ready to use.</p>
        <p>And it is not just any ASP.NET application: it is a reference implementation of the <strong>Pageless ASP.NET Web Forms Architecture</strong> &mdash; no <code>.aspx</code> pages, no code-behind files, no per-page control lifecycle. Every request is routed in <code>Global.asax</code> to a plain handler that queries data and writes HTML, and every response can flow through the same public cache. You keep the deployment simplicity of classic Web Forms while working in a clean, modern, page-less request model. Building your own site as a C# theme <em>is</em> the proof that the architecture holds up in practice.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-list-check'></i> The essentials</h2></div>
    <div class='card-body'>
        <ol>
            <li><strong>Inherit <code>CsTemplate</code>.</strong> Add a class under <code>/engine/CsTemplate/Themes/{YourTheme}/</code> that derives from <code>CsTemplate</code> and override <code>Slug</code> (it must equal the active-theme slug that selects your theme):
                <pre><code>public partial class MyTheme : CsTemplate
{
    public override string Slug { get { return ""my-theme""; } }
    public override string Name { get { return ""My Theme""; } }
}</code></pre>
            </li>
            <li><strong>Keep assets in the standard path.</strong> Put CSS / JavaScript / images in <code>/assets/themes/{theme_name}/</code> &mdash; exactly the same convention as an HTML theme. Link them yourself in your layout, and bump a cache-buster <code>?v=</code> whenever you change one.</li>
            <li><strong>Override only the pages you care about.</strong> Every <code>Handle*</code> method has a default that falls back to the built-in HTML-theme handler, so a partial C# theme transparently inherits folder-theme rendering for anything it does not override.</li>
            <li><strong>Emit through the cache.</strong> Call <code>WriteCached(html)</code> to write a page &mdash; it participates in the public RAM/file page cache just like the HTML path. Writing straight to <code>ApiHelper</code> bypasses the cache.</li>
        </ol>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-database'></i> Getting data &mdash; the unified <code>CsTemplate</code> API</h2></div>
    <div class='card-body'>
        <p>An HTML theme receives data through <code>{{tokens}}</code>. A C# theme instead calls <strong>public methods on <code>CsTemplate</code></strong>. The CMS core centralises and manages the common data here, so theme code gets a stable, intention-revealing API and never has to know the SQL or the connection details.</p>

        <h3 style='margin-top:0'>Identity &amp; metadata <span class='form-hint'>(override)</span></h3>
        <table class='data-table'>
            <thead><tr><th>Member</th><th>Purpose</th></tr></thead>
            <tbody>
                <tr><td><code>Slug</code></td><td><strong>Required.</strong> Must equal the active-theme slug that selects this theme.</td></tr>
                <tr><td><code>Name</code> / <code>Description</code> / <code>Author</code> / <code>Url</code> / <code>Version</code></td><td>Shown in the Themes library (the C# parallel to a folder theme's <code>config.txt</code>).</td></tr>
            </tbody>
        </table>

        <h3>Page handlers <span class='form-hint'>(override; each defaults to the HTML-theme handler)</span></h3>
        <table class='data-table'>
            <thead><tr><th>Method</th><th>Route / role</th></tr></thead>
            <tbody>
                <tr><td><code>HandleHome()</code></td><td>The site root.</td></tr>
                <tr><td><code>HandlePost(string slug)</code></td><td>A single post. Return <code>false</code> for &ldquo;not found&rdquo; (the caller then tries the next match / 404).</td></tr>
                <tr><td><code>HandlePage(string slug)</code></td><td>A single page. Same <code>false</code> = not-found contract.</td></tr>
                <tr><td><code>HandleLatestPost()</code></td><td>The flat latest-posts listing.</td></tr>
                <tr><td><code>HandleBlog()</code></td><td><code>/blog</code> (defaults to the latest-post listing).</td></tr>
                <tr><td><code>HandleCategoriesLatestPost()</code></td><td>The per-category sections page.</td></tr>
                <tr><td><code>HandleCategory(string slug)</code></td><td>A single category listing.</td></tr>
                <tr><td><code>HandleNotFound()</code></td><td>The 404 page.</td></tr>
            </tbody>
        </table>

        <h3>Output &amp; encoding <span class='form-hint'>(protected helpers)</span></h3>
        <table class='data-table'>
            <thead><tr><th>Member</th><th>Use</th></tr></thead>
            <tbody>
                <tr><td><code>WriteCached(string html)</code></td><td>Emit the finished page through the public cache &mdash; the sanctioned output path.</td></tr>
                <tr><td><code>H(string value)</code></td><td>HTML-encode user text (titles, excerpts&hellip;). Always wrap untrusted values &mdash; raw interpolation is an injection risk.</td></tr>
                <tr><td><code>Attr(string value)</code></td><td>Attribute-encode a value for <code>href</code> / <code>src</code> etc.</td></tr>
                <tr><td><code>AssetBase</code></td><td><code>/assets/themes/{Slug}</code> &mdash; build links like <code>AssetBase + ""/style.css""</code>.</td></tr>
                <tr><td><code>WithDb&lt;T&gt;(Func&lt;SQLiteExpress,T&gt; work)</code></td><td>Open + dispose a SQLite connection around your own query (see below).</td></tr>
            </tbody>
        </table>

        <h3>Settings</h3>
        <table class='data-table'>
            <thead><tr><th>Method</th><th>Returns</th></tr></thead>
            <tbody>
                <tr><td><code>GetSiteName()</code></td><td>The configured site name.</td></tr>
                <tr><td><code>GetSiteTagline(fallback)</code> / <code>GetSiteDescription(fallback)</code></td><td>Site tagline / description.</td></tr>
                <tr><td><code>GetSetting(key, fallback)</code></td><td>Any settings value (cached; no query).</td></tr>
                <tr><td><code>GetCountSetting(key, fallback)</code></td><td>A per-listing post-count setting, clamped to 1&ndash;50.</td></tr>
            </tbody>
        </table>

        <h3>Posts, pages &amp; people</h3>
        <table class='data-table'>
            <thead><tr><th>Method</th><th>Returns</th></tr></thead>
            <tbody>
                <tr><td><code>GetRecentPost(int totalPost)</code></td><td>Newest published posts.</td></tr>
                <tr><td><code>GetCategoryRecentPost(int categoryId, int totalPost)</code></td><td>Newest posts in one category.</td></tr>
                <tr><td><code>GetAllCategoriesRecentPost(int totalPost)</code></td><td>Every non-empty category paired with its latest posts.</td></tr>
                <tr><td><code>SearchPosts(string q, int categoryId, int totalPost)</code></td><td>Relevance search (title &gt; excerpt &gt; content &gt; date); <code>categoryId &gt; 0</code> scopes it.</td></tr>
                <tr><td><code>GetPostBySlug(string slug)</code> / <code>GetPageBySlug(string slug)</code></td><td>One published item by slug, or <code>null</code>.</td></tr>
                <tr><td><code>GetUserDisplayName(int userId)</code></td><td>An author's display name (or empty).</td></tr>
                <tr><td><code>GetRelatedPosts(int excludePostId, int categoryId, int totalPost)</code></td><td>&ldquo;Keep reading&rdquo; list: same category first, then latest overall.</td></tr>
                <tr><td><code>ToExcerpt(string content, int maxLength)</code></td><td>Plain-text excerpt of HTML content.</td></tr>
                <tr><td><code>PostExcerpt(obPost p, int maxLength)</code></td><td>A post's excerpt, falling back to a trim of its content.</td></tr>
            </tbody>
        </table>

        <h3>Shared render helpers</h3>
        <table class='data-table'>
            <thead><tr><th>Method</th><th>Renders</th></tr></thead>
            <tbody>
                <tr><td><code>RenderSearchBar(actionPath, q)</code></td><td>The GET search box used by the listing pages.</td></tr>
                <tr><td><code>RenderRowList(posts, showCategory)</code></td><td>A flat row list [thumb | title / excerpt / date / category].</td></tr>
                <tr><td><code>RenderCategorySection(cat, posts)</code></td><td>One category section (feature post + two mini columns).</td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-magnifying-glass-chart'></i> Complex queries &mdash; go straight to SQLite</h2></div>
    <div class='card-body'>
        <p>When the built-in helpers are not enough, query the database directly. Use <code>WithDb</code> so the connection is opened and disposed for you, and run parameterised SQL through <code>SQLiteExpress</code>:</p>
        <pre><code>var rows = WithDb(delegate (SQLiteExpress s)
{
    var p = new Dictionary&lt;string, object&gt; { { ""@cat"", categoryId } };
    return s.GetObjectList&lt;obPost&gt;(
        ""SELECT * FROM posts WHERE category_id=@cat AND is_published=1 ORDER BY date_published DESC LIMIT 20;"", p);
});</code></pre>
        <p>You can also create and query <strong>your own tables</strong> for custom features &mdash; the same SQLite database backs the whole application, so a C# theme can carry its own data model.</p>
        <p class='form-hint'>Always parameterise (<code>@name</code> + a dictionary). Never concatenate user input into SQL.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-plug'></i> Custom pages, settings &amp; API endpoints</h2></div>
    <div class='card-body'>
        <p>To add something the theme handlers do not cover &mdash; a brand-new public page, a custom admin screen, or an API endpoint &mdash; register a route in <code>Global.asax.cs</code> and point it at your handler. The router is a simple <code>switch</code> on the request path:</p>
        <pre><code>case ""/api/my-action"":
    MyApi.HandleRequest(); return;</code></pre>
        <p>A handler reads the request, does its work, and writes a response. For a public, cacheable page use <code>PublicPageCache.WriteAndCache(html)</code>; for an API use <code>ApiHelper</code> (<code>WriteJson</code>, <code>WriteSuccess</code>, <code>WriteError</code>, or write text directly). Gate admin-only handlers with <code>AdminGuard.RequireLogin()</code>.</p>
        <p>This is how you build <strong>automated article import and posting</strong>, scheduled maintenance actions, webhooks, or third-party integrations &mdash; all in plain C#, all page-less.</p>

        <h3 style='margin-bottom:6px'>Worked example &mdash; the article-markdown endpoint</h3>
        <p>The shipped <code>GetArticleMarkdownApi</code> (route <code>/api/get-article-markdown?id=123</code>) is a compact, real example of a public, no-login endpoint: it reads a query parameter, fetches a post, and returns <code>text/plain</code>. For markdown-format posts it returns the stored source; for HTML-format posts it converts on the fly with <code>System.engine.Markdown.HtmlToMarkdown.ToMarkdown(html)</code>. The result is cached in the RAM tier (honoring the <code>cache_ram_enabled</code> setting).</p>
        <p class='form-hint'>Reusable Markdown utilities: <code>MarkdownToHtml.ToHtml(md)</code> and <code>HtmlToMarkdown.ToMarkdown(html)</code> in <code>System.engine.Markdown</code>. See the <a href='/admin/markdown-docs'>Markdown Documentation</a> for exact rendering behavior.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-diagram-project'></i> The shape of a C# page</h2></div>
    <div class='card-body'>
        <p>A handler queries data, builds HTML (typically a shared <code>Layout</code> helper for the head/footer plus the body), and emits it through the cache:</p>
        <pre><code>public override void HandleHome()
{
    var posts = GetRecentPost(GetCountSetting(""home_post_count""));
    var layout = NewLayout(GetSiteName());
    var sb = new StringBuilder();
    sb.Append(layout.RenderHeader());
    foreach (var p in posts)
        sb.Append(""&lt;h2&gt;"" + H(p.Title) + ""&lt;/h2&gt;"");
    sb.Append(layout.RenderFooter());
    WriteCached(sb.ToString());
}</code></pre>
        <p class='form-hint'>See <code>/engine/CsTemplate/Themes/Broadsheet/</code> for the full pattern: a <code>Layout</code> helper, one file per page handler, and a shared article renderer.</p>
    </div>
</div>
";
    }
}
