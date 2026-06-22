using System.Text;

namespace System.engine.RH
{
    // ============================================================
    // /admin/markdown-docs - Markdown syntax & rendering reference.
    //
    // Documents what Hearth's built-in Markdown parser
    // (System.engine.Markdown.MarkdownToHtml) supports and how each
    // construct renders, including the deliberate deviations from
    // CommonMark/GitHub (underscores are never emphasis; C-style escapes
    // like \t are literal). Linked from the content editor's Editor Type
    // selector and from General Guidelines.
    //
    // Plain (non-interpolated) verbatim string so literal Markdown samples
    // render as-is.
    // ============================================================
    public static class AdminMarkdownDocs
    {
        public static void HandleRequest()
        {
            if (!AdminGuard.RequireLogin()) return;

            var tpl = new AdminTemplate
            {
                Title = "Markdown reference",
                ActiveItem = "guidelines",
                PageHeading = "Markdown syntax &amp; rendering"
            };

            var sb = new StringBuilder();
            sb.Append(tpl.RenderHeader());
            sb.Append(Body);
            sb.Append(tpl.RenderFooter());
            ApiHelper.WriteHtml(sb.ToString());
            ApiHelper.EndResponse();
        }

        const string Body = @"
<div class='card'>
    <div class='card-header'><h2><i class='fa-brands fa-markdown'></i> About Hearth's Markdown</h2></div>
    <div class='card-body'>
        <p>When a post or page uses the <strong>Markdown</strong> editor type, its source is converted to HTML by Hearth's own single-pass parser. It supports the common Markdown constructs &mdash; headings, emphasis, lists, tables, code, blockquotes, links, images and more &mdash; with a few <strong>deliberate deviations</strong> from CommonMark / GitHub described below.</p>
        <ul>
            <li><strong>Only <code>*</code> is an emphasis marker.</strong> The underscore <code>_</code> is always literal.</li>
            <li><strong>C-style escapes are not interpreted.</strong> <code>\t</code> and <code>\n</code> stay literal.</li>
            <li><strong>Raw HTML passes through</strong> by default (and is <em>not</em> sanitised &mdash; see the security note).</li>
        </ul>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-bold'></i> Emphasis &mdash; the most important rule</h2></div>
    <div class='card-body'>
        <p><strong>Only the asterisk <code>*</code> creates emphasis. The underscore <code>_</code> is ALWAYS a literal character.</strong></p>
        <table class='data-table'>
            <thead><tr><th>You write</th><th>You get</th></tr></thead>
            <tbody>
                <tr><td><code>*italic*</code></td><td><em>italic</em> &mdash; <code>&lt;em&gt;</code></td></tr>
                <tr><td><code>**bold**</code></td><td><strong>bold</strong> &mdash; <code>&lt;strong&gt;</code></td></tr>
                <tr><td><code>***bold italic***</code></td><td><strong><em>bold italic</em></strong></td></tr>
                <tr><td><code>_italic_</code></td><td>literal <code>_italic_</code> (NOT italic)</td></tr>
                <tr><td><code>snake_case_name</code></td><td>literal <code>snake_case_name</code> (untouched)</td></tr>
                <tr><td><code>__init__</code></td><td>literal <code>__init__</code> (untouched)</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>A deliberate choice: identifiers, file paths and variable names with underscores are never accidentally italicised. The cost: Markdown written elsewhere that uses <code>_</code> for emphasis renders those underscores literally. <strong>When writing for this parser, use <code>*</code> for italics and <code>**</code> for bold.</strong> (Closing-delimiter scanning skips backslash-escaped characters, so <code>*a \* b*</code> still emphasises correctly.)</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-backslash'></i> Escapes &mdash; what backslash does</h2></div>
    <div class='card-body'>
        <p>A backslash escapes only this set of <strong>punctuation</strong> characters, emitting the literal character:</p>
        <pre><code>\ ` * _ { } [ ] ( ) # + - . ! | ~ &gt;</code></pre>
        <p><strong>A backslash before anything else is a literal backslash</strong> &mdash; C-style escape sequences are NOT interpreted:</p>
        <table class='data-table'>
            <thead><tr><th>You write</th><th>You get</th></tr></thead>
            <tbody>
                <tr><td><code>\*</code></td><td>literal <code>*</code></td></tr>
                <tr><td><code>\t</code></td><td>literal <code>\t</code> (backslash + t &mdash; NOT a tab)</td></tr>
                <tr><td><code>\n</code></td><td>literal <code>\n</code> (NOT a newline)</td></tr>
                <tr><td><code>C:\table\tangible</code></td><td>literal <code>C:\table\tangible</code></td></tr>
            </tbody>
        </table>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-heading'></i> Block-level constructs</h2></div>
    <div class='card-body'>
        <h3 style='margin-top:0'>Headings</h3>
        <p><code>#</code> through <code>######</code> (levels 1&ndash;6), and <strong>must</strong> be followed by a space. Trailing <code>#</code> and surrounding whitespace are stripped; content is inline-parsed.</p>
        <pre><code># Title          &rarr; &lt;h1&gt;Title&lt;/h1&gt;
### Section ###  &rarr; &lt;h3&gt;Section&lt;/h3&gt;
#NoSpace         &rarr; literal text, not a heading</code></pre>

        <h3>Paragraphs &amp; line breaks</h3>
        <p>Consecutive non-blank lines form one paragraph until a blank line or a block element begins. Within a paragraph, a single newline becomes <code>&lt;br&gt;</code>. A trailing backslash on a line, or an explicit <code>&lt;br&gt;</code>, also controls line breaks.</p>

        <h3>Horizontal rules</h3>
        <p>A line containing <strong>only</strong> <code>-</code> or <code>*</code> (spaces allowed), with <strong>3 or more</strong> of the character &mdash; emits <code>&lt;hr&gt;</code>. e.g. <code>---</code>, <code>***</code>, <code>- - -</code>.</p>

        <h3>Blockquotes</h3>
        <p>Lines beginning with <code>&gt;</code> (optional leading whitespace, optional one space after). Supports <strong>lazy continuation</strong> (a following non-blank line is pulled in). The inner content is <strong>recursively parsed</strong> and wrapped in <code>&lt;blockquote&gt;</code>.</p>

        <h3>Code</h3>
        <p><strong>Fenced blocks</strong> open and close with three backticks; an optional language tag may follow the opening fence (intended for highlight.js). The body is HTML-encoded.</p>
        <pre><code>```python
print(""hi"")
```
&rarr; &lt;pre&gt;&lt;code class=""language-python""&gt;print(&amp;quot;hi&amp;quot;)&lt;/code&gt;&lt;/pre&gt;</code></pre>
        <p><strong>Inline code</strong> uses backtick runs of any length, matched by an equal-length closing run. If the code both starts and ends with a space, one space is trimmed from each side. Body is HTML-encoded. Use double backticks when the code itself contains a backtick.</p>

        <h3>Lists</h3>
        <p><strong>Unordered:</strong> markers <code>-</code>, <code>*</code> or <code>+</code> followed by a space. <strong>Ordered:</strong> digits then <code>.</code> or <code>)</code> then a space (<code>1.</code> or <code>1)</code>).</p>
        <p><strong>Nesting:</strong> content indented beyond the list's base indent becomes child content of the preceding item; it is dedented and <strong>recursively parsed</strong>, so nested lists, paragraphs and code inside items all work. A tab counts as 4 spaces.</p>

        <h3>Tables (GitHub-style)</h3>
        <p>Triggered when a line contains an unescaped <code>|</code> <strong>and</strong> the next line is a separator row (only <code>|</code>, <code>-</code>, <code>:</code>, spaces). Column alignment comes from colons:</p>
        <table class='data-table'>
            <thead><tr><th>Separator</th><th>Alignment</th></tr></thead>
            <tbody>
                <tr><td><code>:---</code></td><td>left</td></tr>
                <tr><td><code>---:</code></td><td>right</td></tr>
                <tr><td><code>:---:</code></td><td>center</td></tr>
                <tr><td><code>---</code></td><td>default</td></tr>
            </tbody>
        </table>
        <p class='form-hint'>Escape a literal pipe inside a cell with <code>\|</code>. Header cells become <code>&lt;th&gt;</code>, body cells <code>&lt;td&gt;</code>; the whole table is wrapped in a horizontally scrollable container. Cell contents are inline-parsed.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-link'></i> Inline constructs</h2></div>
    <div class='card-body'>
        <h3 style='margin-top:0'>Links &amp; images</h3>
        <pre><code>[text](url)             &rarr; &lt;a href=""url""&gt;text&lt;/a&gt;
[text](url ""title"")     &rarr; adds title=""title""
![alt](url)             &rarr; &lt;img src=""url"" alt=""alt""&gt;
![alt](url ""title"")     &rarr; adds title=""title""</code></pre>
        <p class='form-hint'>Titles may use <code>&quot;&hellip;&quot;</code> or <code>'&hellip;'</code>. Link text is inline-parsed (nested brackets handled by depth matching). URLs and titles are attribute-encoded.</p>

        <h3>Strikethrough</h3>
        <p><code>~~struck~~</code> &rarr; <code>&lt;del&gt;struck&lt;/del&gt;</code></p>

        <h3>Auto-links</h3>
        <p>Bare URLs starting with <code>http://</code> or <code>https://</code> become links automatically. Trailing punctuation (<code>. , ; : ! ? )</code>) is trimmed off the URL.</p>

        <h3>HTML entities</h3>
        <p>Valid entities pass through unchanged; a bare <code>&amp;</code> becomes <code>&amp;amp;</code>. A lone <code>&gt;</code> in text becomes <code>&amp;gt;</code>; a <code>&lt;</code> becomes <code>&amp;lt;</code> unless it begins a passed-through HTML tag.</p>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-code'></i> Raw HTML passthrough</h2></div>
    <div class='card-body'>
        <p><strong>Raw HTML is allowed by default.</strong> A line beginning with <code>&lt;</code> followed by a recognised block tag is emitted verbatim along with following lines, until <strong>whichever comes first</strong>: the matching close tag (depth-aware, so nested same-name tags balance), or a blank line. Markdown placed right after a closing tag is parsed normally &mdash; no blank line required:</p>
        <pre><code>&lt;div&gt;raw&lt;/div&gt;
# Heading        &rarr; &lt;div&gt;raw&lt;/div&gt; then &lt;h1&gt;Heading&lt;/h1&gt;</code></pre>
        <p>Recognised block tags include <code>div, p, table, ul, ol, li, h1&ndash;h6, pre, blockquote, form, iframe, script, style, section, article, figure, details, video, audio, svg</code> and more. Inline tag-like <code>&lt;&hellip;&gt;</code> sequences also pass through.</p>
        <div class='card' style='margin-top:12px;border-color:var(--warn,#d97706)'>
            <div class='card-body'>
                <p style='margin:0'><strong><i class='fa-solid fa-triangle-exclamation'></i> Security note.</strong> With passthrough on, the parser does <strong>not</strong> sanitise HTML &mdash; including <code>&lt;script&gt;</code> and <code>&lt;iframe&gt;</code>. Markdown content is authored by trusted admins, so this is intended; just be aware that whatever HTML you put in the source is emitted as-is.</p>
            </div>
        </div>
    </div>
</div>

<div class='card'>
    <div class='card-header'><h2><i class='fa-solid fa-circle-check'></i> Quick do / don't</h2></div>
    <div class='card-body'>
        <ul>
            <li>&#9989; Use <code>*italic*</code> and <code>**bold**</code>.</li>
            <li>&#10060; Don't use <code>_italic_</code> / <code>__bold__</code> &mdash; underscores stay literal.</li>
            <li>&#9989; Use backslash escapes only for the punctuation set listed above.</li>
            <li>&#10060; Don't expect <code>\t</code> or <code>\n</code> to become whitespace &mdash; they're literal.</li>
            <li>&#9989; Headings, list markers and <code>&gt;</code> need a trailing space.</li>
            <li>&#9989; Use <code>\|</code> for a literal pipe inside a table cell.</li>
            <li>&#9888;&#65039; Raw HTML is passed through unsanitised &mdash; only put in what you trust.</li>
        </ul>
    </div>
</div>
";
    }
}
