/* =========================================================
   WYSIWYG Editor - vanilla JS
   Renders into #editor; bridges to a hidden <textarea data-editor-source>.
   No font-family is applied; inherits from the parent CMS.
   Requires: FontAwesome (for toolbar icons) and (optionally)
   media-browser.js loaded BEFORE this file (for image picking).
   ========================================================= */

(function () {
    'use strict';

    function init() {
        var editor = document.getElementById('editor');
        if (!editor) return;
        if (editor.dataset.wysiwygInited === '1') return;
        editor.dataset.wysiwygInited = '1';

        var content  = editor.querySelector('.wysiwyg__content');
        var source   = editor.querySelector('.wysiwyg__source');
        var status   = editor.querySelector('.wysiwyg__status-mode');
        var wordEl   = editor.querySelector('.wysiwyg__status-count');
        var tableBar = editor.querySelector('.wysiwyg__table-tools');

        // ---- Source textarea bridge (the form-bound field that gets posted) ----
        var sourceTextarea = document.querySelector('[data-editor-source]');

        var savedRange = null;

        function saveSelection() {
            var sel = window.getSelection();
            if (sel && sel.rangeCount > 0 && content.contains(sel.anchorNode)) {
                savedRange = sel.getRangeAt(0).cloneRange();
            }
        }
        function restoreSelection() {
            if (!savedRange) return;
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(savedRange);
        }
        function focusContent() {
            content.focus();
            restoreSelection();
        }
        function exec(cmd, value) {
            focusContent();
            document.execCommand(cmd, false, value || null);
            updateState();
        }

        // Track caret/selection
        content.addEventListener('keyup', onSelectionChanged);
        content.addEventListener('mouseup', onSelectionChanged);
        content.addEventListener('focus', onSelectionChanged);
        content.addEventListener('input', function () { updateWordCount(); });
        document.addEventListener('selectionchange', function () {
            if (document.activeElement === content) {
                saveSelection();
                updateActiveButtons();
                updateTableBar();
            }
        });

        function onSelectionChanged() {
            saveSelection();
            updateActiveButtons();
            updateTableBar();
            updateBlockSelect();
        }
        function updateState() {
            saveSelection();
            updateActiveButtons();
            updateTableBar();
            updateBlockSelect();
            updateWordCount();
        }
        function updateActiveButtons() {
            var buttons = editor.querySelectorAll('.wysiwyg__btn[data-cmd]');
            buttons.forEach(function (btn) {
                var cmd = btn.getAttribute('data-cmd');
                try { btn.classList.toggle('is-active', document.queryCommandState(cmd)); }
                catch (e) {}
            });
        }
        function updateBlockSelect() {
            var sel = window.getSelection();
            if (!sel.rangeCount) return;
            var node = sel.anchorNode;
            if (!node) return;
            if (node.nodeType === 3) node = node.parentNode;
            var blockTags = ['p', 'h1', 'h2', 'h3', 'h4', 'blockquote'];
            var el = node;
            while (el && el !== content) {
                if (blockTags.indexOf(el.tagName ? el.tagName.toLowerCase() : '') !== -1) {
                    var select = editor.querySelector('[data-format-block]');
                    if (select) select.value = el.tagName.toLowerCase();
                    return;
                }
                el = el.parentNode;
            }
        }
        function updateWordCount() {
            if (!wordEl) return;
            var text = content.innerText.trim();
            var words = text ? text.split(/\s+/).length : 0;
            wordEl.textContent = words + ' word' + (words === 1 ? '' : 's');
        }

        // ---------- Button bindings ----------
        editor.querySelectorAll('.wysiwyg__btn[data-cmd]').forEach(function (btn) {
            btn.addEventListener('mousedown', function (e) { e.preventDefault(); });
            btn.addEventListener('click', function () { exec(btn.getAttribute('data-cmd')); });
        });

        editor.querySelectorAll('.wysiwyg__color').forEach(function (input) {
            input.addEventListener('mousedown', saveSelection);
            input.addEventListener('input', function () {
                exec(input.getAttribute('data-cmd'), input.value);
            });
        });

        var formatBlock = editor.querySelector('[data-format-block]');
        if (formatBlock) {
            formatBlock.addEventListener('mousedown', saveSelection);
            formatBlock.addEventListener('change', function () { exec('formatBlock', formatBlock.value); });
        }

        editor.querySelectorAll('.wysiwyg__btn[data-action]').forEach(function (btn) {
            btn.addEventListener('mousedown', function (e) {
                e.preventDefault();
                saveSelection();
            });
            btn.addEventListener('click', function () {
                var action = btn.getAttribute('data-action');
                if (action === 'image')  openImageModal();
                if (action === 'link')   openLinkModal();
                if (action === 'table')  openTableModal();
                if (action === 'code')   openCodeModal();
                if (action === 'source') toggleSource();
            });
        });

        // ---------- Source view toggle ----------
        function toggleSource() {
            var isSource = editor.classList.toggle('is-source');
            if (isSource) {
                source.value = content.innerHTML;
                if (status) status.textContent = 'HTML Source';
                source.focus();
            } else {
                content.innerHTML = source.value;
                hydrateCodeBlocks();
                if (status) status.textContent = 'Visual';
                content.focus();
                updateWordCount();
            }
        }

        // ---------- Modal helpers ----------
        function openModal(id) {
            saveSelection();
            document.getElementById(id).classList.add('is-open');
        }
        function closeModal(modal) {
            modal.classList.remove('is-open');
            focusContent();
        }
        document.querySelectorAll('.wy-modal').forEach(function (modal) {
            modal.addEventListener('click', function (e) {
                if (e.target === modal) closeModal(modal);
            });
            modal.querySelectorAll('[data-close]').forEach(function (btn) {
                btn.addEventListener('click', function () { closeModal(modal); });
            });
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                document.querySelectorAll('.wy-modal.is-open').forEach(function (m) { closeModal(m); });
            }
        });

        // ---------- Image dialog ----------
        var imgModal   = document.getElementById('image-modal');
        var imgUrl     = document.getElementById('img-url');
        var imgAlt     = document.getElementById('img-alt');
        var imgWidth   = document.getElementById('img-width');
        var imgHeight  = document.getElementById('img-height');
        var imgPreview = document.getElementById('div-img-preview');

        function updateImagePreview() {
            if (!imgPreview) return;
            var url = imgUrl.value.trim();
            if (url) {
                imgPreview.style.backgroundImage = "url('" + url.replace(/'/g, "%27") + "')";
                imgPreview.setAttribute('data-has-image', 'true');
            } else {
                imgPreview.style.backgroundImage = '';
                imgPreview.removeAttribute('data-has-image');
            }
        }
        if (imgUrl) imgUrl.addEventListener('input', updateImagePreview);

        function openImageModal() {
            if (!imgModal) return;
            imgUrl.value = '';
            imgAlt.value = '';
            imgWidth.value = '';
            imgHeight.value = '';
            updateImagePreview();
            openModal('image-modal');
            setTimeout(function () { imgUrl.focus(); }, 50);
        }

        async function pickFromMedia() {
            if (!window.mediaBrowser || typeof window.mediaBrowser.pick !== 'function') {
                var url = window.prompt('Paste an image URL:');
                if (url) { imgUrl.value = url; updateImagePreview(); }
                return;
            }
            try {
                var url = await window.mediaBrowser.pick({ accept: ['image/*'] });
                if (url) {
                    imgUrl.value = url;
                    updateImagePreview();
                }
            } catch (err) {
                console.error('Media picker error:', err);
            }
        }
        var btnTop = document.getElementById('btn-select-media-top');
        var btnBot = document.getElementById('btn-select-media-bottom');
        if (btnTop) btnTop.addEventListener('click', pickFromMedia);
        if (btnBot) btnBot.addEventListener('click', pickFromMedia);

        var imgInsert = document.getElementById('img-insert');
        if (imgInsert) imgInsert.addEventListener('click', function () {
            var url = imgUrl.value.trim();
            if (!url) { imgUrl.focus(); return; }
            var attrs = 'src="' + escapeAttr(url) + '"';
            if (imgAlt.value.trim())    attrs += ' alt="' + escapeAttr(imgAlt.value.trim()) + '"';
            if (imgWidth.value.trim())  attrs += ' width="' + escapeAttr(imgWidth.value.trim()) + '"';
            if (imgHeight.value.trim()) attrs += ' height="' + escapeAttr(imgHeight.value.trim()) + '"';
            focusContent();
            document.execCommand('insertHTML', false, '<img ' + attrs + ' />');
            closeModal(imgModal);
        });

        // ---------- Link dialog ----------
        var linkModal = document.getElementById('link-modal');
        function openLinkModal() {
            if (!linkModal) return;
            var sel = window.getSelection();
            var selectedText = sel && sel.toString();
            document.getElementById('link-url').value = '';
            document.getElementById('link-text').value = selectedText || '';
            document.getElementById('link-newtab').checked = false;
            openModal('link-modal');
        }
        var linkInsert = document.getElementById('link-insert');
        if (linkInsert) linkInsert.addEventListener('click', function () {
            var url     = document.getElementById('link-url').value.trim();
            var text    = document.getElementById('link-text').value.trim();
            var newTab  = document.getElementById('link-newtab').checked;
            if (!url) return;
            focusContent();
            var html;
            var target = newTab ? ' target="_blank" rel="noopener"' : '';
            if (text) {
                html = '<a href="' + escapeAttr(url) + '"' + target + '>' + escapeHtml(text) + '</a>';
                document.execCommand('insertHTML', false, html);
            } else {
                var sel = window.getSelection();
                if (sel && sel.toString()) {
                    document.execCommand('createLink', false, url);
                    if (newTab) {
                        var node = sel.anchorNode;
                        while (node && node.nodeName !== 'A') node = node.parentNode;
                        if (node) {
                            node.setAttribute('target', '_blank');
                            node.setAttribute('rel', 'noopener');
                        }
                    }
                } else {
                    html = '<a href="' + escapeAttr(url) + '"' + target + '>' + escapeHtml(url) + '</a>';
                    document.execCommand('insertHTML', false, html);
                }
            }
            closeModal(linkModal);
        });

        // ---------- Table dialog ----------
        var tableModal = document.getElementById('table-modal');
        function openTableModal() { if (tableModal) openModal('table-modal'); }
        var tblInsert = document.getElementById('tbl-insert');
        if (tblInsert) tblInsert.addEventListener('click', function () {
            var rows = Math.max(1, parseInt(document.getElementById('tbl-rows').value, 10) || 1);
            var cols = Math.max(1, parseInt(document.getElementById('tbl-cols').value, 10) || 1);
            var header = document.getElementById('tbl-header').checked;
            var html = '<table>';
            var r = 0;
            if (header) {
                html += '<thead><tr>';
                for (var c = 0; c < cols; c++) html += '<th>Header ' + (c + 1) + '</th>';
                html += '</tr></thead>';
                r = 1;
            }
            html += '<tbody>';
            for (; r < rows; r++) {
                html += '<tr>';
                for (var c2 = 0; c2 < cols; c2++) html += '<td>&nbsp;</td>';
                html += '</tr>';
            }
            html += '</tbody></table><p><br></p>';
            focusContent();
            document.execCommand('insertHTML', false, html);
            closeModal(tableModal);
        });

        // ---------- Table contextual ops ----------
        function getCurrentCell() {
            var sel = window.getSelection();
            if (!sel.rangeCount) return null;
            var node = sel.anchorNode;
            while (node && node !== content) {
                if (node.nodeName === 'TD' || node.nodeName === 'TH') return node;
                node = node.parentNode;
            }
            return null;
        }
        function updateTableBar() {
            editor.classList.toggle('has-table', !!getCurrentCell());
        }
        if (tableBar) {
            tableBar.addEventListener('mousedown', function (e) { e.preventDefault(); });
            tableBar.addEventListener('click', function (e) {
                var btn = e.target.closest('[data-table]');
                if (!btn) return;
                var op = btn.getAttribute('data-table');
                var cell = getCurrentCell();
                if (!cell) return;
                var row = cell.parentNode;
                var table = cell.closest('table');
                var colIndex = Array.prototype.indexOf.call(row.children, cell);

                if (op === 'insertRowAbove' || op === 'insertRowBelow') {
                    var newRow = document.createElement('tr');
                    for (var i = 0; i < row.children.length; i++) {
                        var td = document.createElement('td');
                        td.innerHTML = '&nbsp;';
                        newRow.appendChild(td);
                    }
                    row.parentNode.insertBefore(newRow, op === 'insertRowAbove' ? row : row.nextSibling);
                }
                if (op === 'insertColLeft' || op === 'insertColRight') {
                    var allRows = table.querySelectorAll('tr');
                    allRows.forEach(function (r2) {
                        var ref = r2.children[colIndex];
                        var isHead = r2.parentNode.tagName === 'THEAD';
                        var newCell = document.createElement(isHead ? 'th' : 'td');
                        newCell.innerHTML = isHead ? 'Header' : '&nbsp;';
                        if (op === 'insertColLeft') r2.insertBefore(newCell, ref);
                        else r2.insertBefore(newCell, ref ? ref.nextSibling : null);
                    });
                }
                if (op === 'deleteRow') {
                    if (table.querySelectorAll('tr').length > 1) row.parentNode.removeChild(row);
                }
                if (op === 'deleteCol') {
                    var allRows2 = table.querySelectorAll('tr');
                    if (allRows2[0] && allRows2[0].children.length > 1) {
                        allRows2.forEach(function (r3) {
                            if (r3.children[colIndex]) r3.removeChild(r3.children[colIndex]);
                        });
                    }
                }
                if (op === 'deleteTable') {
                    table.parentNode.removeChild(table);
                }
                updateTableBar();
                content.focus();
            });
        }

        // ---------- Code block dialog ----------
        var codeModal = document.getElementById('code-modal');
        function openCodeModal() {
            if (!codeModal) return;
            document.getElementById('code-content').value = '';
            document.getElementById('code-lang').value = 'javascript';
            openModal('code-modal');
        }
        var codeInsert = document.getElementById('code-insert');
        if (codeInsert) codeInsert.addEventListener('click', function () {
            var lang = document.getElementById('code-lang').value;
            var raw  = document.getElementById('code-content').value;
            if (!raw) return;
            var escaped = escapeHtml(raw);
            var html = '<pre><code class="language-' + lang + '">' + escaped + '</code></pre><p><br></p>';
            focusContent();
            document.execCommand('insertHTML', false, html);
            hydrateCodeBlocks();
            closeModal(codeModal);
        });

        function hydrateCodeBlocks() {
            if (typeof hljs === 'undefined') return;
            content.querySelectorAll('pre code').forEach(function (block) {
                block.removeAttribute('data-highlighted');
                try { hljs.highlightElement(block); } catch (e) {}
            });
        }

        // ---------- Paste sanitization ----------
        content.addEventListener('paste', function (e) {
            if (e.shiftKey) return;
            var html = (e.clipboardData || window.clipboardData).getData('text/html');
            var text = (e.clipboardData || window.clipboardData).getData('text/plain');
            if (html) {
                e.preventDefault();
                var clean = sanitizeHtml(html);
                document.execCommand('insertHTML', false, clean);
            }
        });
        function sanitizeHtml(html) {
            var doc = new DOMParser().parseFromString(html, 'text/html');
            doc.querySelectorAll('script, style, meta, link').forEach(function (n) { n.remove(); });
            doc.querySelectorAll('*').forEach(function (n) {
                n.removeAttribute('style');
                [].slice.call(n.attributes).forEach(function (a) {
                    if (a.name.indexOf('on') === 0) n.removeAttribute(a.name);
                });
            });
            return doc.body.innerHTML;
        }

        // ---------- Helpers ----------
        function escapeHtml(str) {
            return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
        }
        function escapeAttr(str) {
            return String(str).replace(/"/g, '&quot;').replace(/</g, '&lt;');
        }

        // ---------- Keyboard shortcuts ----------
        content.addEventListener('keydown', function (e) {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
                e.preventDefault();
                openLinkModal();
            }
        });

        // ---------- Public API ----------
        editor.getHTML = function () {
            return editor.classList.contains('is-source') ? source.value : content.innerHTML;
        };
        editor.setHTML = function (html) {
            content.innerHTML = html || '';
            if (editor.classList.contains('is-source')) source.value = content.innerHTML;
            hydrateCodeBlocks();
            updateWordCount();
        };

        // ---------- Init from source textarea ----------
        if (sourceTextarea) {
            editor.setHTML(sourceTextarea.value);

            // Sync editor → textarea before the form is submitted, so the
            // existing FormData-based save handler ships the rich content.
            var form = sourceTextarea.form;
            if (form) {
                form.addEventListener('submit', function () {
                    sourceTextarea.value = editor.getHTML();
                }, true); // capture phase: run before other submit handlers
            }
        }

        hydrateCodeBlocks();
        updateWordCount();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
