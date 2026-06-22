using System.Text;

namespace System.engine.RH
{
    // ============================================================
    // /admin/themes/docs - the theme authoring reference.
    //
    // Hearth renders the public site by dropping live data into
    // {{token}} placeholders inside external HTML template files.
    // The templates carry NO logic - no loops, no if-statements -
    // only structure, markup and tokens. The C# engine decides what
    // each token contains; the theme owns layout and style.
    //
    // This page documents the fixed file set every theme follows,
    // the exact token vocabulary the engine fills for each file
    // (verified against the live handlers), and sample data so an
    // author knows what shape each value takes.
    //
    // The whole body is a plain (non-interpolated) verbatim string
    // so the literal {{token}} examples render as-is.
    // ============================================================
    public static class AdminThemeDocs
    {
        public static void HandleRequest()
        {
            if (!AdminGuard.RequireLogin()) return;

            var tpl = new AdminTemplate
            {
                Title = "Theme guide",
                ActiveItem = "themes",
                PageHeading = "Theme authoring guide",
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
    <a href='/admin/themes/docs' class='btn btn-sm btn-primary'>HTML Template Guide</a>
    <a href='/admin/themes/docs-csharp' class='btn btn-sm btn-ghost'>C# Template Guide</a>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-layer-group'></i> Two template engines</h2></div>
    <div class='card-body'>
        <p>Hearth CMS ships with <strong>two template engines</strong>, and you choose whichever fits the site you are building.</p>

        <h3 style='margin-top:0'>HTML-based templates</h3>
        <ul>
            <li><strong>Effortless to edit.</strong> A theme is just plain HTML files with <code>{{token}}</code> placeholders &mdash; no compilation and no build step.</li>
            <li><strong>Fixed data shape.</strong> The data each template receives is predefined &mdash; well suited to a straightforward content site.</li>
            <li><strong>Directly editable home page.</strong> The landing page can be edited in place; the only real limit is your own imagination and creativity.</li>
            <li><strong>The classic CMS.</strong> A quick, familiar way to present articles &mdash; content management in its most recognisable form.</li>
        </ul>

        <h3>C#-based templates</h3>
        <ul>
            <li><strong>Full flexibility.</strong> Render any layout you can express in code &mdash; there is no fixed token vocabulary to work around.</li>
            <li><strong>Unlimited UI potential.</strong> Build complex, interactive components, call third-party APIs, and compose pages however you like.</li>
            <li><strong>More than a CMS.</strong> At full power this is essentially application development, not just publishing.</li>
            <li><strong>Custom data access.</strong> Run your own SQLite queries for bespoke, purpose-built features.</li>
            <li><strong>Custom endpoints.</strong> Add your own API endpoints for custom actions &mdash; automated article import and posting, integrations, scheduled jobs, and more.</li>
            <li><strong>Application scaffolding.</strong> Hearth's foundation is a fast on-ramp for building a small application from the ground up on SQLite &mdash; and not just any ASP.NET app, but one that follows the new <strong>Pageless ASP.NET Web Forms Architecture</strong>.</li>
        </ul>

        <p>To build a C#-based template, read the <a href='/admin/themes/docs-csharp'>C# Template Guide</a>.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-circle-exclamation'></i> While editing an HTML template</h2></div>
    <div class='card-body'>
        <ul>
            <li><strong>Turn the RAM cache off while editing</strong> so your live changes appear immediately, then turn it back on when you are done (Settings &rarr; Cache).</li>
            <li><strong>Apply a cache-buster</strong> to your own CSS / JavaScript (bump the <code>?v=</code> or rename the file) so browsers fetch the new version rather than a stale copy &mdash; details in the linking section below.</li>
        </ul>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-fire'></i> How theming works in Hearth</h2></div>
    <div class='card-body'>
        <p>A <strong>theme</strong> is a small set of <strong>HTML template files</strong> plus whatever <strong>CSS, JavaScript, fonts and images</strong> you want to ship with it. The public site is rendered by dropping live data into <code>{{token}}</code> placeholders inside those templates &mdash; there is <strong>no logic</strong> in the HTML: no loops, no <code>if</code> statements, no expressions. The engine decides what each token contains; your job is purely <em>structure</em> and <em>style</em>.</p>
        <p>The model is deliberately small. There is <strong>one skeleton</strong> (the body shell), <strong>six page templates</strong> the system knows how to fill, and a handful of <strong>components</strong> &mdash; the small repeating pieces (a post card, a list row) that the engine stamps out once per item and joins together.</p>
        <ul>
            <li><strong>No logic in templates.</strong> Only <code>{{token}}</code> placeholders. An unknown or misspelled token simply renders as empty text &mdash; it never errors.</li>
            <li><strong>No escaping to worry about.</strong> The engine escapes every value before it reaches the template. Plain-text tokens are HTML-encoded, URL tokens are attribute-encoded, and pre-built HTML blocks (nav, lists, rendered article bodies) are injected verbatim. You never call an encoder in a template.</li>
            <li><strong>Single-pass substitution.</strong> Tokens are replaced in one scan, so HTML injected through a token is never re-scanned for more tokens. No accidental double-substitution.</li>
            <li><strong>Per-file fallback.</strong> If your theme is missing a template file, the engine falls back to the built-in default theme's copy of that same file. A half-finished theme never breaks the site &mdash; pages always render.</li>
            <li><strong>You own your assets.</strong> CSS, JS, fonts, background and decoration images live in your theme's public asset folder, and <strong>you link them yourself</strong> in <code>_layout.html</code>. The engine does not generate the stylesheet link and does not manage cache-busting (see below).</li>
        </ul>
        <p class='form-hint'><strong>hearth</strong> is the name of this CMS, and also the slug of the first default theme. Throughout this guide, wherever you see <code>{slug}</code>, substitute your own theme's slug (e.g. <code>hearth</code>).</p>
        <div class='card' style='margin-top:14px'>
            <div class='card-body'>
                <p><strong><i class='fa-solid fa-shield-halved'></i> Strongly recommended: duplicate an existing theme before modifying anything.</strong> Do not edit the built-in <code>hearth</code> theme (or any shipped theme) in place. Copy <em>both</em> of its folders to a new slug of your own &mdash; the templates <em>and</em> the assets:</p>
                <pre><code>/App_Data/themes/{theme_name}/    &rarr;  /App_Data/themes/{my_theme}/
/assets/themes/{theme_name}/      &rarr;  /assets/themes/{my_theme}/</code></pre>
                <p>The reason is <strong>update safety</strong>: when you update the CMS to a newer version, the updater overwrites the original shipped theme files &mdash; any edits you made directly inside them are lost. Your duplicated folders use your own slug, so an update never touches them and your modified templates and assets survive every upgrade.</p>
                <p><strong><i class='fa-solid fa-rotate'></i> And every time you edit a CSS/JS/image asset, apply a cache-buster</strong> (bump the <code>?v=</code> on that asset's link, or rename the file) so your readers' browsers actually fetch the new version instead of serving the old one from cache. Details in the linking section below.</p>
            </div>
        </div>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-folder-tree'></i> Where the files live</h2></div>
    <div class='card-body'>
        <p><strong>Templates</strong> live on disk under the server-only data folder. This folder is <em>not</em> web-accessible, so your raw <code>{{token}}</code> markup is never exposed over HTTP. All first-level templates sit in the theme root; the small repeating pieces go in a <code>components/</code> sub-folder:</p>
        <pre><code>/App_Data/themes/{slug}/
    _layout.html                    &larr; the skeleton (the body shell)
    home.html                       &larr; landing option: home
    latest-post.html                &larr; landing option: flat list of latest posts
    categories-latest-post.html     &larr; landing option: per-category sections
    category.html                   &larr; a single category page
    article-full-width.html         &larr; page/post layout: full width
    article-sidebar.html            &larr; page/post layout: with sidebar aside
    components/
        section-latest-posts.html   &larr; the home ""latest writing"" wrapper
        post-card.html              &larr; one card in the home grid
        row-post.html               &larr; one row in a flat list
        category-section.html       &larr; one category block (feature + 2 columns)
        cat-mini-item.html          &larr; one compact item inside a category column
        footer-column.html          &larr; one footer column</code></pre>

        <p><strong>Assets</strong> (CSS, JS, fonts, images) live in the web-accessible asset folder for your theme. You decide the filenames and the structure here &mdash; the engine does not look inside this folder:</p>
        <pre><code>/assets/themes/{slug}/
    site.css        (or whatever you name it; you link it yourself)
    theme.js        (optional)
    fonts/ ...
    img/ ...        (backgrounds, decoration, etc.)</code></pre>

        <p class='form-hint'>The reference theme to copy from is <code>/App_Data/themes/hearth/</code> with its assets at <code>/assets/themes/hearth/</code>.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-link'></i> Linking CSS &amp; JS &mdash; you own it, including cache-busting</h2></div>
    <div class='card-body'>
        <p>The engine <strong>does not</strong> inject a stylesheet link and <strong>does not</strong> generate a version query string. There is no <code>{{theme_href}}</code> token. Instead you write your own <code>&lt;link&gt;</code> and <code>&lt;script&gt;</code> tags directly in <code>_layout.html</code>, pointing at files in your asset folder:</p>
        <pre><code>&lt;link rel='stylesheet' href='/assets/themes/hearth/site.css' /&gt;
&lt;script src='/assets/themes/hearth/theme.js' defer&gt;&lt;/script&gt;</code></pre>

        <p>Because you own these tags, <strong>cache-busting is your responsibility</strong>. When you ship a change and want browsers to re-fetch a file, bump a version query string by hand on whichever asset changed &mdash; CSS, JS, an image, a font, anything:</p>
        <pre><code>&lt;link rel='stylesheet' href='/assets/themes/hearth/site.css?v=4' /&gt;
&lt;script src='/assets/themes/hearth/theme.js?v=2' defer&gt;&lt;/script&gt;
&lt;!-- in CSS or markup --&gt;  background-image: url('/assets/themes/hearth/img/grain.png?v=2');</code></pre>

        <p>This is intentional: a CMS-generated version number can only track one thing and tends to over- or under-invalidate. A theme author knows exactly which file changed and can bump precisely that one. Keep the numbers small and human &mdash; they only need to change when <em>you</em> publish a new asset.</p>
        <p class='form-hint'>Tip: if you'd rather not touch query strings, you can also rename the file (<code>site-v4.css</code>) and update the link &mdash; same effect, fully under your control.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-sitemap'></i> The shape of a rendered page</h2></div>
    <div class='card-body'>
        <p>Every public URL produces a page in two nested stages:</p>
        <ol>
            <li><strong>The skeleton</strong> &mdash; <code>_layout.html</code> &mdash; is rendered first. It is the <code>&lt;html&gt;</code> / <code>&lt;head&gt;</code> / header / footer shell, with one big <code>{{body}}</code> hole in the middle.</li>
            <li><strong>A page template</strong> &mdash; one of the six below &mdash; is rendered and dropped into that <code>{{body}}</code>.</li>
            <li>Some tokens inside a page template are themselves <strong>containers</strong>. The engine fills them by stamping a <strong>component</strong> once per item and joining the results. For example <code>{{row_list}}</code> in <code>latest-post.html</code> is filled with many <code>components/row-post.html</code> renders, one per post.</li>
        </ol>
        <pre><code>_layout.html  ({{body}})
   &rarr; home.html
        &rarr; {{latest_posts_block}}  &rarr; components/section-latest-posts.html
             &rarr; {{post_card_list}}   &rarr; components/post-card.html  (&times; N posts)
</code></pre>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-house-chimney'></i> Choosing the landing page</h2></div>
    <div class='card-body'>
        <p>Under <strong>Settings</strong>, an admin chooses what the site root (<code>/</code> and <code>/home</code>) renders. The choice maps to one of these templates:</p>
        <table class='data-table'>
            <thead><tr><th>Setting</th><th>Renders</th></tr></thead>
            <tbody>
                <tr><td>Home</td><td><code>home.html</code> &mdash; the hero + ""latest writing"" landing</td></tr>
                <tr><td>Latest posts</td><td><code>latest-post.html</code> &mdash; a flat list of the newest posts</td></tr>
                <tr><td>Categories</td><td><code>categories-latest-post.html</code> &mdash; one section per category</td></tr>
                <tr><td>A specific Page</td><td>any published <em>page</em> you pick, rendered through the article layout</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>Whichever you pick, the standalone routes still exist too (<code>/latest-post</code>, <code>/categories-latest-post</code>); the setting just <em>also</em> serves the chosen one at the root.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-file-lines'></i> Pages and posts</h2></div>
    <div class='card-body'>
        <p>Hearth has two kinds of dynamic content, and they work <strong>identically</strong>:</p>
        <ul>
            <li><strong>Page</strong> &mdash; standalone content (About, Contact). Served at <code>/{slug}</code>. No category. No published-date line and no &ldquo;keep reading&rdquo; aside.</li>
            <li><strong>Post</strong> &mdash; blog/news content. Served at <code>/{slug}</code>. Has a category, a published date, and a &ldquo;keep reading&rdquo; aside of related posts. On a slug clash a page takes priority over a post.</li>
        </ul>
        <p>Both a page and a post are rendered by the <strong>same two article templates</strong>, and each can choose either layout:</p>
        <table class='data-table'>
            <thead><tr><th>Layout</th><th>Template</th><th>Notes</th></tr></thead>
            <tbody>
                <tr><td>Full width (<code>stack</code>)</td><td><code>article-full-width.html</code></td><td>Content runs the full column; any aside sits below.</td></tr>
                <tr><td>Sidebar (<code>split</code>)</td><td><code>article-sidebar.html</code></td><td>Content beside a sidebar that holds the aside.</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>The layout is the real axis &mdash; <em>not</em> whether the content is a page or a post. A page defaults to full-width, a post to sidebar, but the editor (and a <code>?layout=stack|split</code> preview override) can pick either.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-cube'></i> _layout.html &mdash; the skeleton</h2></div>
    <div class='card-body'>
        <p>The shell every page is wrapped in. You write the entire <code>&lt;head&gt;</code> here, including your own stylesheet and script links (see the linking section above). <code>{{body}}</code> is <strong>required</strong> and is where the page template lands.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains / sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{head_meta}}</code></td><td>raw</td><td>The page's <code>&lt;title&gt;</code> and meta-description, built by the engine. Place inside <code>&lt;head&gt;</code>.<br><span class='form-hint'>e.g. <code>&lt;title&gt;My Post - Hearth&lt;/title&gt;&lt;meta name='description' content='...'&gt;</code></span></td></tr>
                <tr><td><code>{{site_name}}</code></td><td>text</td><td>The site name. <span class='form-hint'>e.g. <code>Hearth</code></span></td></tr>
                <tr><td><code>{{nav_items}}</code></td><td>raw</td><td>The rendered top-nav links (admin-managed nav builder, up to 2 levels).</td></tr>
                <tr><td><code>{{footer_column_list}}</code></td><td>raw</td><td>The multi-column footer area, or empty when no columns are configured. Built from <code>components/footer-column.html</code>.</td></tr>
                <tr><td><code>{{footer_text}}</code></td><td>text</td><td>The footer line. <span class='form-hint'>e.g. <code>&copy; 2026 Hearth. All rights reserved.</code></span></td></tr>
                <tr><td><code>{{body}}</code></td><td>raw</td><td><strong>Required.</strong> The rendered page template.</td></tr>
            </tbody>
        </table>
        <p class='form-hint' style='margin-top:14px'>There is no <code>{{theme_href}}</code> and no <code>{{theme_js}}</code> token &mdash; link your own assets directly. Everything else you want in <code>&lt;head&gt;</code> (fonts, icon CSS, syntax-highlight CSS/JS) you also add yourself; see <code>hearth/_layout.html</code> for a complete working example.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-house'></i> home.html</h2></div>
    <div class='card-body'>
        <p>The home landing body. The &ldquo;latest writing&rdquo; section is optional &mdash; when there are no posts the engine passes an empty string, so you never get a heading with no cards.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains / sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{site_name}}</code></td><td>text</td><td><span class='form-hint'>e.g. <code>Hearth</code></span></td></tr>
                <tr><td><code>{{site_tagline}}</code></td><td>text</td><td><span class='form-hint'>e.g. <code>A clean place to write.</code></span></td></tr>
                <tr><td><code>{{site_description}}</code></td><td>text</td><td><span class='form-hint'>e.g. <code>Welcome to our minimalist CMS.</code></span></td></tr>
                <tr><td><code>{{latest_posts_block}}</code></td><td>raw</td><td>The whole ""latest writing"" section, or empty when there are no posts. Built from <code>components/section-latest-posts.html</code>.</td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-rectangle-list'></i> latest-post.html</h2></div>
    <div class='card-body'>
        <p>A flat list of posts, with a search box. Also used to render search results (the <code>?q=</code> form posts back to the same route). During a search, <code>{{page_subheading}}</code> is empty and <code>{{search_meta}}</code> shows the result count.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains / sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{page_heading}}</code></td><td>text</td><td><span class='form-hint'>e.g. <code>Latest posts</code></span></td></tr>
                <tr><td><code>{{page_subheading}}</code></td><td>raw</td><td>Sub-heading paragraph, or empty during search. <span class='form-hint'>e.g. <code>&lt;p class='list-sub'&gt;Fresh writing, newest first.&lt;/p&gt;</code></span></td></tr>
                <tr><td><code>{{search_bar}}</code></td><td>raw</td><td>The GET search form.</td></tr>
                <tr><td><code>{{search_meta}}</code></td><td>raw</td><td>&ldquo;N result(s) for &hellip;&rdquo; line, or empty when not searching.</td></tr>
                <tr><td><code>{{row_list}}</code></td><td>raw</td><td>The list of rows, or an empty-state message. Built from <code>components/row-post.html</code>.</td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-tag'></i> category.html</h2></div>
    <div class='card-body'>
        <p>A single category at <code>/category/{slug}</code> &mdash; the same flat-list shape as <code>latest-post.html</code>, plus a breadcrumb. Search here is scoped to the category.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains / sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{breadcrumbs}}</code></td><td>raw</td><td>Breadcrumb nav. <span class='form-hint'>e.g. Home / Categories / Essays</span></td></tr>
                <tr><td><code>{{page_heading}}</code></td><td>text</td><td>The category name. <span class='form-hint'>e.g. <code>Essays</code></span></td></tr>
                <tr><td><code>{{page_subheading}}</code></td><td>raw</td><td><span class='form-hint'>e.g. <code>&lt;p class='list-sub'&gt;Posts in this category.&lt;/p&gt;</code></span></td></tr>
                <tr><td><code>{{search_bar}}</code></td><td>raw</td><td>The GET search form (scoped to this category).</td></tr>
                <tr><td><code>{{search_meta}}</code></td><td>raw</td><td>Result count line, or empty.</td></tr>
                <tr><td><code>{{row_list}}</code></td><td>raw</td><td>The list of rows. Built from <code>components/row-post.html</code>.</td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-layer-group'></i> categories-latest-post.html</h2></div>
    <div class='card-body'>
        <p>One section per category, newest items first. Each section is built from <code>components/category-section.html</code>.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains / sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{page_heading}}</code></td><td>text</td><td><span class='form-hint'>e.g. <code>Browse by category</code></span></td></tr>
                <tr><td><code>{{page_subheading}}</code></td><td>text</td><td><span class='form-hint'>e.g. <code>The latest from every category.</code></span></td></tr>
                <tr><td><code>{{category_section_list}}</code></td><td>raw</td><td>One block per category, or an empty-state. Built from <code>components/category-section.html</code>.</td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-newspaper'></i> article-full-width.html &amp; article-sidebar.html</h2></div>
    <div class='card-body'>
        <p>Both article templates share the <strong>same token set</strong>. The sidebar layout places <code>{{article_aside}}</code> beside the content; the full-width layout places it after. Several tokens are <em>block-or-nothing</em> &mdash; they render a complete element when there is data, or empty when there isn't (so pages can omit dates and asides cleanly).</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains / sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{breadcrumbs}}</code></td><td>raw</td><td>Breadcrumb nav. <span class='form-hint'>Post: Home / Blog / Essays / Title &middot; Page: Home / Title</span></td></tr>
                <tr><td><code>{{article_title}}</code></td><td>text</td><td>The article title. <span class='form-hint'>e.g. <code>Welcome to Hearth</code></span></td></tr>
                <tr><td><code>{{published_date}}</code></td><td>raw</td><td>Published-date line, or empty (pages have none). <span class='form-hint'>e.g. <code>&lt;div class='doc-meta'&gt;&hellip; January 1, 2026&lt;/div&gt;</code></span></td></tr>
                <tr><td><code>{{updated_date}}</code></td><td>raw</td><td>&ldquo;Updated &hellip;&rdquo; line, or empty when no modified date.</td></tr>
                <tr><td><code>{{article_author}}</code></td><td>raw</td><td>Author line, or empty when the author has no display name.</td></tr>
                <tr><td><code>{{cover_image}}</code></td><td>raw</td><td>The complete cover-image element (the <code>components/cover-image.html</code> partial, a <code>.content-cover-image</code> wrapper around an <code>&lt;img&gt;</code>), or empty when there is no cover &mdash; so pages without one omit it cleanly. Edit <code>components/cover-image.html</code> to change the markup. <span class='form-hint'>e.g. <code>&lt;div class='content-cover-image'&gt;&lt;img src='/uploads/x.jpg' alt='' /&gt;&lt;/div&gt;</code></span></td></tr>
                <tr><td><code>{{article_content}}</code></td><td>raw</td><td><strong>The rendered article body</strong> (Markdown already converted to HTML, or HTML passed through).</td></tr>
                <tr><td><code>{{article_aside}}</code></td><td>raw</td><td>The &ldquo;keep reading&rdquo; recent-posts block (posts only), or empty for pages and when there is nothing to show.</td></tr>
            </tbody>
        </table>
        <p class='form-hint' style='margin-top:14px'>In the full-width template you may place <code>{{article_aside}}</code> anywhere &mdash; or omit it entirely if your design doesn't want a related-posts block.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-puzzle-piece'></i> Components &mdash; the repeating pieces</h2></div>
    <div class='card-body'>
        <p>A component is the markup for a <strong>single item</strong>. The engine renders it once per record and joins the results, then injects the joined block into a container token on a page template. You only ever describe one item.</p>

        <h3 style='margin-top:0'>components/section-latest-posts.html <span class='form-hint'>(home ""latest writing"" wrapper)</span></h3>
        <p>A wrapper, rendered once. Holds the grid container.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains</th></tr></thead>
            <tbody>
                <tr><td><code>{{post_card_list}}</code></td><td>raw</td><td>The joined post cards (many <code>post-card.html</code> renders).</td></tr>
            </tbody>
        </table>

        <h3>components/post-card.html <span class='form-hint'>(one card in the home grid)</span></h3>
        <p>Joined into <code>{{post_card_list}}</code> above.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Sample</th></tr></thead>
            <tbody>
                <tr><td><code>{{post_url}}</code></td><td>url</td><td><code>/welcome-to-hearth</code></td></tr>
                <tr><td><code>{{post_title}}</code></td><td>text</td><td><code>Welcome to Hearth</code></td></tr>
                <tr><td><code>{{post_excerpt}}</code></td><td>text</td><td><code>Your first post &mdash; edit or delete&hellip;</code></td></tr>
                <tr><td><code>{{post_date}}</code></td><td>text</td><td><code>Jan 1, 2026</code></td></tr>
            </tbody>
        </table>

        <h3>components/row-post.html <span class='form-hint'>(one row in a flat list)</span></h3>
        <p>Joined into <code>{{row_list}}</code> on <code>latest-post.html</code> and <code>category.html</code>.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Sample / note</th></tr></thead>
            <tbody>
                <tr><td><code>{{post_url}}</code></td><td>url</td><td><code>/design-philosophy</code></td></tr>
                <tr><td><code>{{post_title}}</code></td><td>text</td><td><code>Design Philosophy</code></td></tr>
                <tr><td><code>{{post_excerpt}}</code></td><td>text</td><td><code>Less, but better.</code></td></tr>
                <tr><td><code>{{post_date}}</code></td><td>text</td><td><code>Jan 1, 2026</code></td></tr>
                <tr><td><code>{{post_category}}</code></td><td>raw</td><td>A category tag element, or empty (category pages hide it). <span class='form-hint'>e.g. <code>&lt;span class='row-cat'&gt;&hellip; Essays&lt;/span&gt;</code></span></td></tr>
                <tr><td><code>{{post_thumb_empty}}</code></td><td>raw</td><td>Class suffix: <code> is-empty</code> when there is no cover image, else empty. Put it right after the thumb's class name.</td></tr>
                <tr><td><code>{{post_thumb_img}}</code></td><td>raw</td><td>A ready-made <code>&lt;img&gt;</code> for the cover (sized to cover its wrapper via your CSS), or empty when there is no cover. <span class='form-hint'>e.g. <code>&lt;img src='/uploads/x.jpg' alt='' /&gt;</code></span></td></tr>
            </tbody>
        </table>
        <p class='form-hint'>The thumb pattern is <code>&lt;div class='row-thumb{{post_thumb_empty}}'&gt;{{post_thumb_img}}&lt;/div&gt;</code> &mdash; a wrapper <code>&lt;div&gt;</code> you style, holding the engine's <code>&lt;img&gt;</code>. Give the wrapper a fixed size plus <code>overflow:hidden</code>, then set the inner <code>&lt;img&gt;</code> to <code>width:100%; height:100%; object-fit:cover</code> so the photo fills it. <code>{{post_thumb_empty}}</code> still appends <code>is-empty</code> when there is no cover, for styling the placeholder.</p>

        <h3>components/category-section.html <span class='form-hint'>(one category block)</span></h3>
        <p>Joined into <code>{{category_section_list}}</code> on <code>categories-latest-post.html</code>. The first (newest) post is the <em>feature</em>; the rest are split across two compact columns.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Sample / note</th></tr></thead>
            <tbody>
                <tr><td><code>{{category_title}}</code></td><td>text</td><td><code>Essays</code></td></tr>
                <tr><td><code>{{category_url}}</code></td><td>url</td><td><code>/category/essays</code></td></tr>
                <tr><td><code>{{feature_url}}</code></td><td>url</td><td><code>/welcome-to-hearth</code></td></tr>
                <tr><td><code>{{feature_title}}</code></td><td>text</td><td><code>Welcome to Hearth</code></td></tr>
                <tr><td><code>{{feature_excerpt}}</code></td><td>text</td><td><code>Your first post&hellip;</code></td></tr>
                <tr><td><code>{{feature_date}}</code></td><td>text</td><td><code>Jan 1, 2026</code></td></tr>
                <tr><td><code>{{feature_thumb_empty}}</code></td><td>raw</td><td>Class suffix <code> is-empty</code> for the feature image, or empty.</td></tr>
                <tr><td><code>{{feature_thumb_img}}</code></td><td>raw</td><td>A ready-made <code>&lt;img&gt;</code> for the feature cover (cover-fit via your CSS), or empty.</td></tr>
                <tr><td><code>{{column_1_list}}</code></td><td>raw</td><td>First compact column &mdash; joined <code>cat-mini-item.html</code> renders.</td></tr>
                <tr><td><code>{{column_2_list}}</code></td><td>raw</td><td>Second compact column &mdash; joined <code>cat-mini-item.html</code> renders.</td></tr>
            </tbody>
        </table>

        <h3>components/cat-mini-item.html <span class='form-hint'>(compact item in a category column)</span></h3>
        <p>Joined into <code>{{column_1_list}}</code> / <code>{{column_2_list}}</code> above.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Sample / note</th></tr></thead>
            <tbody>
                <tr><td><code>{{post_url}}</code></td><td>url</td><td><code>/design-philosophy</code></td></tr>
                <tr><td><code>{{post_title}}</code></td><td>text</td><td><code>Design Philosophy</code></td></tr>
                <tr><td><code>{{post_date}}</code></td><td>text</td><td><code>Jan 1, 2026</code></td></tr>
                <tr><td><code>{{post_thumb_empty}}</code></td><td>raw</td><td>Class suffix <code> is-empty</code>, or empty.</td></tr>
                <tr><td><code>{{post_thumb_img}}</code></td><td>raw</td><td>A ready-made <code>&lt;img&gt;</code> (cover-fit via your CSS), or empty.</td></tr>
            </tbody>
        </table>

        <h3>components/footer-column.html <span class='form-hint'>(one footer column)</span></h3>
        <p>Rendered once per configured footer column and joined into <code>{{footer_column_list}}</code> on <code>_layout.html</code>.</p>
        <table class='data-table'>
            <thead><tr><th>Token</th><th>Type</th><th>Contains</th></tr></thead>
            <tbody>
                <tr><td><code>{{footer_content}}</code></td><td>raw</td><td>That column's content (admin Markdown rendered to HTML).</td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-list-check'></i> Building a theme, step by step</h2></div>
    <div class='card-body'>
        <ol>
            <li><strong>Duplicate a theme</strong> in the Theme editor (start from <code>hearth</code>). This creates the theme record and gives you a <code>{slug}</code>.</li>
            <li><strong>Create the folders</strong> <code>/App_Data/themes/{slug}/</code> and <code>/App_Data/themes/{slug}/components/</code> on the server, and the asset folder <code>/assets/themes/{slug}/</code>.</li>
            <li><strong>Copy only the templates you want to change</strong> from <code>hearth</code>. Anything you omit falls back to the default automatically &mdash; you can override just <code>_layout.html</code> and <code>home.html</code> and inherit the rest.</li>
            <li><strong>Add your assets</strong> to <code>/assets/themes/{slug}/</code> and link them yourself in <code>_layout.html</code>.</li>
            <li><strong>Activate</strong> the theme in the Theme editor.</li>
        </ol>
        <p class='form-hint'>If you inherit any template from <code>hearth</code> but write your own CSS, keep the original class names (<code>doc-grid</code>, <code>row-post</code>, <code>post-card</code>, <code>cat-section</code>, <code>cat-feature</code>, <code>cat-mini</code>, &hellip;) or that inherited markup will be unstyled.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-copy'></i> Duplicating &amp; deleting themes</h2></div>
    <div class='card-body'>
        <p>A theme is just <strong>two folders that share the same slug</strong> &mdash; one for the templates, one for the assets. There is no database table and no registry: the slug <em>is</em> the folder name. That makes both duplicating and deleting a theme plain folder operations on the server.</p>
        <pre><code>/App_Data/themes/{slug}/      &larr; templates (server-only)
/assets/themes/{slug}/        &larr; CSS, JS, fonts, images (web-accessible)</code></pre>

        <h3 style='margin-top:0'>Duplicating a theme</h3>
        <p><strong>Duplicating is the strongly recommended way to start any modification.</strong> Never edit a shipped theme (like <code>hearth</code>) in place: a future CMS update will overwrite the original shipped files and wipe your edits. A duplicate under your own slug &mdash; both <code>/App_Data/themes/{theme_name}/</code> <em>and</em> <code>/assets/themes/{theme_name}/</code> &mdash; is never touched by an update, so your modified templates and assets are kept across upgrades.</p>
        <p>To create a new theme, make the two folders for your new slug, then either write fresh files or copy an existing theme's files into them and modify from there:</p>
        <ol>
            <li><strong>Pick a slug</strong> for the new theme &mdash; the folder name. Use lowercase letters, numbers and hyphens (e.g. <code>my-theme</code>).</li>
            <li><strong>Create the two folders:</strong>
                <pre><code>/App_Data/themes/{new_theme_name}/
/assets/themes/{new_theme_name}/</code></pre>
            </li>
            <li><strong>Add the templates</strong> &mdash; one of two ways:
                <ul>
                    <li><em>Start from scratch:</em> write only the template files you need into <code>/App_Data/themes/{new_theme_name}/</code>. Anything you omit falls back to the default <code>hearth</code> theme automatically, so even a single <code>_layout.html</code> is a working theme.</li>
                    <li><em>Copy an existing theme:</em> copy the contents of, say, <code>/App_Data/themes/hearth/</code> (including its <code>components/</code> sub-folder) into your new theme folder, then edit the files. Do the same for its assets &mdash; copy <code>/assets/themes/hearth/</code> into <code>/assets/themes/{new_theme_name}/</code>.</li>
                </ul>
            </li>
            <li><strong>Fix the asset links.</strong> If you copied another theme, its <code>_layout.html</code> still points at the old asset folder (e.g. <code>/assets/themes/hearth/site.css</code>). Update those <code>&lt;link&gt;</code> / <code>&lt;script&gt;</code> paths to your new slug: <code>/assets/themes/{new_theme_name}/site.css</code>.</li>
            <li><strong>Activate</strong> the new theme from the Themes library when you're ready.</li>
        </ol>
        <p class='form-hint'>The new theme appears in the Themes library as soon as the <code>/App_Data/themes/{new_theme_name}/</code> folder exists &mdash; the library lists folders, so no extra registration step is needed.</p>

        <h3>Deleting a theme</h3>
        <p>To delete a theme, simply remove its two folders:</p>
        <pre><code>/App_Data/themes/{target_theme}/      &larr; delete
/assets/themes/{target_theme}/        &larr; delete</code></pre>
        <p>Once both folders are gone the theme disappears from the Themes library. (The Themes library also offers a Delete button that does exactly this for non-active themes.)</p>
        <ul>
            <li><strong>Don't delete the active theme.</strong> Activate a different theme first &mdash; otherwise the site falls back to the default <code>hearth</code> theme for every missing file.</li>
            <li><strong>Don't delete <code>hearth</code>.</strong> It is the built-in fallback every other theme relies on for any template it doesn't override; removing it can leave partial themes unable to render.</li>
            <li>Deleting is permanent &mdash; there is no recycle bin. Keep a copy of the folders if you might want the theme back.</li>
        </ul>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-triangle-exclamation'></i> Tips &amp; gotchas</h2></div>
    <div class='card-body'>
        <ul>
            <li><strong>An unknown token renders empty.</strong> A typo like <code>{{titel}}</code> silently produces nothing &mdash; check spelling against the tables above.</li>
            <li><strong>Thumbnails are an <code>&lt;img&gt;</code> in a wrapper you own.</strong> The engine drops a bare <code>&lt;img&gt;</code> for each cover (or nothing) into the <code>{{post_thumb_img}}</code> / <code>{{feature_thumb_img}}</code> tokens. Wrap it in your own sized <code>&lt;div&gt;</code> with <code>overflow:hidden</code> and give the inner <code>&lt;img&gt;</code> <code>width:100%; height:100%; object-fit:cover</code> so the photo fills the box. No <code>loading</code> attribute is set, so thumbnails load immediately.</li>
            <li><strong>Type matters.</strong> <em>text</em> tokens are HTML-encoded (safe inside elements), <em>url</em> tokens are attribute-encoded (safe inside <code>href</code>/<code>src</code>), and <em>raw</em> tokens are whole HTML blocks &mdash; place them where block-level HTML is valid, never inside a plain-text context.</li>
            <li><strong>Suffix tokens have no leading space.</strong> <code>{{thumb_empty}}</code> sits flush after a class name (it appends <code>is-empty</code>); the cover itself arrives through the separate <code>{{thumb_img}}</code> token as a complete <code>&lt;img&gt;</code> element you place inside the wrapper.</li>
            <li><strong>Number of posts</strong> shown on each list/section is set in <em>Settings</em>, not in templates.</li>
            <li><strong>Templates are server-only.</strong> Files under <code>/App_Data/</code> can't be fetched over HTTP, so your raw <code>{{token}}</code> markup is never exposed.</li>
            <li><strong>Never modify a shipped theme in place &mdash; duplicate it first.</strong> A CMS update overwrites the original theme files (both <code>/App_Data/themes/{theme_name}/</code> and <code>/assets/themes/{theme_name}/</code>); your duplicated copy under its own slug survives every update.</li>
            <li><strong>You own cache-busting &mdash; every edit needs one.</strong> Whenever you change a CSS/JS/image asset, bump its <code>?v=</code> (or rename the file) so the new edit gets flushed to your readers' browsers; the engine won't do it for you.</li>
        </ul>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-wand-magic-sparkles'></i> Editing the home page (per-theme content)</h2></div>
    <div class='card-body'>
        <p>A theme can expose <strong>editable regions on its home page</strong>, edited visually from the theme editor's <strong>Edit Home Content</strong> button. It loads your live <code>home.html</code> in a frame and lets an admin edit the marked regions in place, with a floating <em>Save</em>.</p>
        <p>You decide what is editable by adding a <code>data-edit</code> attribute (or a typed variant &mdash; <code>data-edit-href</code>, <code>data-edit-src</code>, <code>data-edit-bg</code>, <code>data-edit-icon</code>) to elements in <code>home.html</code>. The element's existing content is the <strong>default</strong>; an admin's overrides are saved per-theme to <code>/App_Data/themes/{slug}/home.values.json</code> and applied when the home page renders. Nothing is stored in the database, and the override travels with the theme folder.</p>

        <h3 style='margin-top:0'>Editable text &mdash; available now</h3>
        <p>Put <code>data-edit='key'</code> on a <strong>plain-text leaf element</strong> (one whose content is just text). The admin edits it inline; the text is saved under <code>key</code>.</p>
        <pre><code>&lt;h3 data-edit='feat1_title'&gt;Made to read&lt;/h3&gt;
&lt;p  data-edit='feat1_body'&gt;Warm typography, generous margins&hellip;&lt;/p&gt;</code></pre>
        <p class='form-hint'>Leaf elements only. If a button holds an icon &mdash; e.g. <code>Read the blog &lt;i class='fa-solid fa-arrow-right'&gt;&lt;/i&gt;</code> &mdash; wrap just the words in a <code>&lt;span data-edit='&hellip;'&gt;</code> so the icon survives the swap.</p>

        <h3>Links, images, backgrounds &amp; icons &mdash; live now</h3>
        <p>Links, images and icons aren't <em>text</em> &mdash; they live in an <strong>attribute</strong> (<code>href</code>, <code>src</code>, an inline <code>style</code> background, the <code>fa-</code> class). Mark the element with a typed attribute and the editor opens the right tool when you click it. All of these <strong>work today</strong>; overrides save to <code>home.values.json</code> exactly like text.</p>
        <table class='data-table'>
            <thead><tr><th>Marker</th><th>Edits</th><th>Click opens</th></tr></thead>
            <tbody>
                <tr><td><code>data-edit='key'</code></td><td>the element's inner text</td><td>inline editing (type in place)</td></tr>
                <tr><td><code>data-edit-href='key'</code></td><td>its <code>href</code></td><td>a URL popover</td></tr>
                <tr><td><code>data-edit-src='key'</code></td><td>its <code>src</code> (an <code>&lt;img&gt;</code>)</td><td>the Media browser</td></tr>
                <tr><td><code>data-edit-bg='key'</code></td><td>its <code>background-image</code> (inline style)</td><td>the Media browser</td></tr>
                <tr><td><code>data-edit-icon='key'</code></td><td>its Font Awesome <code>class</code></td><td>the icon picker</td></tr>
            </tbody>
        </table>
        <pre><code>&lt;a   data-edit-href='cta_url'   href='/blog'&gt;&lt;span data-edit='cta_label'&gt;Read the blog&lt;/span&gt;&lt;/a&gt;
&lt;img data-edit-src='hero_image' src='/assets/themes/{slug}/img/hero.jpg' alt='' /&gt;
&lt;i   data-edit-icon='hero_icon' class='fa-solid fa-fire'&gt;&lt;/i&gt;
&lt;div data-edit-bg='banner_bg' style='background-image:url(&quot;/uploads/banner.jpg&quot;)'&gt;&hellip;&lt;/div&gt;</code></pre>

        <h3>The two kinds of image</h3>
        <p>An image shows up in one of two ways, and <strong>both</strong> are editable:</p>
        <ul>
            <li><strong><code>&lt;img&gt;</code> &mdash; <code>data-edit-src</code></strong>: a real image element. The picked URL is written to its <code>src</code>.</li>
            <li><strong>Background &mdash; <code>data-edit-bg</code></strong>: an element with an <em>inline</em> <code>style='background-image:url(&hellip;)'</code>. The engine swaps only the <code>background-image</code> and <strong>keeps your other inline styles</strong> (size, position, radius&hellip;).</li>
        </ul>
        <p class='form-hint'>A background set in a <strong>CSS file</strong> (e.g. a theme's parallax-hero rule) is <em>not</em> reachable &mdash; only an <strong>inline</strong> <code>background-image</code> on an element in <code>home.html</code> is. To make a CSS background editable, move it onto the element as an inline style and add <code>data-edit-bg</code>.</p>

        <h3>How editing feels</h3>
        <ul>
            <li><strong>Click a region, get the right tool</strong> &mdash; text edits inline; a link opens a URL popover; an image or background opens the Media browser; an icon opens the icon picker (a searchable grid, or paste any <code>fa-</code> class).</li>
            <li><strong>Navigation is locked while editing</strong> &mdash; clicking links and buttons edits them instead of navigating away, so you never lose your place.</li>
            <li><strong>Regions are outlined by type</strong> while the editor is open, and the floating <em>Save</em> writes them all at once.</li>
            <li><strong>Output is escaped</strong> &mdash; text is HTML-encoded, URLs and classes attribute-encoded &mdash; so saved content can't break your markup.</li>
        </ul>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-photo-film'></i> Media, icons &amp; favicon</h2></div>
    <div class='card-body'>
        <h3 style='margin-top:0'>Media browser &mdash; swapping images</h3>
        <p>Image fields use the built-in <strong>MediaBrowser</strong> component (<code>/js/media-browser.js</code>) &mdash; the same picker used across the admin. It returns a URL you store as the field value; the engine writes that into the element's <code>src</code>. You don't build a picker; you call this one:</p>
        <pre><code>// pick a single image
const url  = await mediaBrowser.pick({ accept: ['image/*'] });
// pick several
const urls = await mediaBrowser.pick({ multiSelect: true, accept: ['image/*'] });
// viewer only, or mount inline
await mediaBrowser.open();
const handle = mediaBrowser.mount('#div_media_container'); handle.unmount();</code></pre>
        <p>It talks to the existing media API &mdash; the same endpoints the Media page uses:</p>
        <pre><code>GET  /api/admin/media?action=list
     -&gt; { success:true, html: ""&lt;div class='media-tile' data-url='&hellip;'&gt;&hellip;&lt;/div&gt;&hellip;"" }
POST /api/admin/media   action=upload, file=&lt;File&gt;   -&gt; { success, &hellip; }
POST /api/admin/media   action=delete, id=&lt;int&gt;      -&gt; { success, message? }</code></pre>
        <p class='form-hint'>Each <code>.media-tile</code> exposes <code>data-url</code> (and optionally <code>data-name</code>, <code>data-id</code>). Because the picker already exists, image swapping <strong>reuses</strong> it rather than adding anything new.</p>

        <h3>Font Awesome is loaded site-wide</h3>
        <p>The Font Awesome stylesheet is linked on <strong>every page</strong> &mdash; public site and admin. Use any <code>fa-solid fa-*</code> or <code>fa-regular fa-*</code> class directly in your templates; you never add the library yourself. An icon field simply swaps the <code>fa-</code> classes on an <code>&lt;i&gt;</code>.</p>

        <h3>Favicon is a site-wide setting</h3>
        <p>The favicon is <strong>not</strong> a theme or home-page concern &mdash; it shows on every page across the whole site, the admin included. Set it in <strong>Settings &rarr; Site identity &rarr; Favicon</strong>, using the same Media browser picker (with a live preview). It is stored once globally and injected into every page's <code>&lt;head&gt;</code> automatically &mdash; the correct <code>type</code> is inferred from the file extension (<code>.png</code>, <code>.svg</code>, <code>.ico</code>, &hellip;) &mdash; and is shared by all themes, so it never belongs in <code>home.html</code> or the home editor.</p>
    </div>
</div>
";
    }
}
