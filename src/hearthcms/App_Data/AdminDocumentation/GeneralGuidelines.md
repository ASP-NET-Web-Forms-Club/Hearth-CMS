# General Guidelines

## About this page

Operational notes for running this site. Each section is a short how-to for a task that isn't part of day-to-day writing.

## Documentation & guides

- [HTML Template Guide](/admin/themes/docs) — authoring token-based HTML themes.
- [C# Template Guide](/admin/themes/docs-csharp) — building code-rendered themes by inheriting `CsTemplate` (and scaffolding an application on the Pageless ASP.NET Web Forms architecture).
- [Markdown Documentation](/admin/markdown-docs) — the supported Markdown syntax and how each construct renders.

## Migrating content in via the API (WordPress & batch import)

The CMS exposes a small HTTP API that a migration tool (or any script/agent) can drive to create posts, upload media, and create categories autonomously — for example to move a WordPress site over. The recommended flow is: convert each article to **Markdown**, upload its images first so you know their final URLs, rewrite the image links in the Markdown to those URLs, then create the post.

### Media path format

All imported media is stored under a predictable, date-based path:

```
/media/{year}/{month}/{media_filename}
```

The month is always two digits (e.g. `/media/2026/06/photo.jpg`). Because the year and month are supplied by you on upload, the tool can compute the final URL of every image *before* uploading and rewrite the article body up front.

### Endpoints

| Purpose | Method & URL | Body |
| --- | --- | --- |
| Upload image(s) | `POST /api/migration-import` | `multipart/form-data` |
| Create / update category | `POST /api/admin/categories` | `multipart/form-data` or form-encoded |
| Create / update post | `POST /api/admin/posts` | `multipart/form-data` or form-encoded |

Every endpoint returns JSON. Success looks like `{ "success": true, … }`; failures return `{ "success": false, "message": "…" }` with an HTTP 4xx/5xx status.

### 1. Upload images — `/api/migration-import`

Send the file part(s) plus the target year and month. The response gives you the final root-relative `url` of each file to drop back into your Markdown.

| Field | Notes |
| --- | --- |
| `filebytes` | The file part. Send several file parts in one request to upload many at once. |
| `year` | 4-digit year, e.g. `2026`. |
| `month` | 1–2 digit month, e.g. `6` or `06` (normalised to two digits). |
| `filename` | Optional desired name. For multi-file requests, repeat the field index-aligned with the files; omitted entries fall back to the uploaded part's own name. |
| `build_thumbnail` | Optional `true`/`1` to queue a background thumbnail. |

Allowed image types: `.jpg .jpeg .png .gif .webp .svg .bmp .ico`. Names are sanitized, and a collision in the same folder appends `-2`, `-3`, …. Use this for both **feature/cover images** and **in-line article images** — they all land in the same `/media/{year}/{month}/` tree.

### 2. Categories — `/api/admin/categories`

Posts reference a category by numeric id. Before creating posts, you need each category's id. There are two ways to get it:

**List existing categories** — send `action=list` to discover ids, names and slugs (useful for mapping WordPress categories and for re-runs):

```json
{
  "success": true,
  "data": {
    "count": 2,
    "categories": [
      { "id": 1, "name": "News", "slug": "news", "description": "", "sort_order": 0, "post_count": 12 },
      { "id": 2, "name": "Guides", "slug": "guides", "description": "", "sort_order": 0, "post_count": 5 }
    ]
  }
}
```

**Create a category** — send `action=save`. Keep the returned id.

| Field | Notes |
| --- | --- |
| `action` | `list` to read all; `save` to create/update. |
| `name` | Required for `save`. The display name. |
| `slug` | Optional; auto-derived from the name when omitted. |
| `description`, `cover_image`, `sort_order` | Optional. |

`save` returns `{ "success": true, "data": { "id": <categoryId>, "slug": "…" } }`. Use that `id` as `category_id` when creating posts.

If a category with the same slug already exists, `save` does **not** create a duplicate: it returns `{ "success": false, "existing_id": <id>, "existing_slug": "…", "existing_name": "…" }`. An importer can simply reuse `existing_id`, which makes re-running the migration safe.

### 3. Create the post — `/api/admin/posts`

| Field | Notes |
| --- | --- |
| `action` | `save` |
| `title` | Required. |
| `content` | The article body (your converted Markdown, with image URLs already rewritten to `/media/…`). |
| `content_format` | `markdown` to store and render the body as Markdown (anything else is treated as `html`). |
| `slug` | Optional; auto-derived from the title when omitted. Must be unique. |
| `excerpt` | Optional plain-text summary (Markdown/HTML is stripped). |
| `cover_image` | The feature image URL, e.g. the `/media/…` URL returned in step 1. |
| `category_id` | Numeric category id (`0` = uncategorized). An id that doesn't resolve is treated as uncategorized. |
| `is_published` | `1` to publish, `0` for draft. |
| `date_published` | Optional explicit publish date (preserve the original WordPress date here). Parsed as a date/time, e.g. `2024-03-15` or `2024-03-15 09:30:00`. When omitted on a published post, the current time is stamped. |
| `layout` | Optional: `split` (default) or `stack`. |

To *update* an existing post instead of creating one, include its `id`.

### ⚠️ Authentication — required for the import to work

Every API endpoint above is protected exactly like the rest of the admin: the request must be **authenticated**. A migration tool posting autonomously has no login session, so by default these calls are rejected with `401 Not signed in`. There are three ways to let an automated importer through:

- **API token (recommended for unattended / agent-driven posting).** In [Settings](/admin/settings) → **API access token**, generate a token and copy it. Send it with each API request as the `X-Api-Token` header, or as an `api_token` query-string or form field. The CMS checks it alongside the normal login, so a script, an unattended pipeline, or an AI agent submitting via an MCP/tool call can post **without Dev Mode and without a human login**.

  ```powershell
  # PowerShell example: post an article with the token in a header
  $headers = @{ "X-Api-Token" = "YOUR_TOKEN_HERE" }
  $form = @{
      action         = "save"
      title          = "My migrated article"
      content        = "# Hello`n`nBody in **Markdown**..."
      content_format = "markdown"
      is_published   = "1"
      date_published = "2024-03-15 09:30:00"
      cover_image    = "/media/2024/03/feature.jpg"
  }
  Invoke-RestMethod -Uri "https://your-site/api/admin/posts" -Method Post -Headers $headers -Form $form
  ```

  > ⚠️ Treat the token like a password: it grants full admin-level create/modify access. Prefer the header over the query string (query strings can end up in server logs). Always serve the API over HTTPS, and regenerate the token if it may have leaked — regenerating invalidates the old one immediately.

- **Enable Dev Mode** (simplest for a local, one-off migration). In [Settings](/admin/settings) → **Dev / Testing**, turn on *Dev Mode*. While it's on, the request pipeline auto-logs the first admin user, so API calls succeed **without login credentials**. Run your import, then **turn Dev Mode back off**.

  > ⚠️ Never leave Dev Mode enabled on a public/production site — with it on, anyone who reaches an admin or API URL is treated as the admin, with no password. Do the migration locally, or take the site offline for the duration.

- **Authenticate the request yourself.** Your tool can instead carry a valid admin session — for example by performing the normal login first and reusing the session cookie on subsequent API calls.

### Building the client (MCP tool / PowerShell)

The CMS side is now complete: predictable endpoints, a media path it understands, and token auth for unattended callers. The remaining work is on *your* side — a small client that speaks this protocol: it converts each article to Markdown, uploads the images (`/api/migration-import`), rewrites the in-line and feature image URLs to the returned `/media/…` paths, then creates the post (`/api/admin/posts`), sending the API token on every request. That client can be a custom MCP server tool, a PowerShell script, or any HTTP client.

## Changing the admin login path

The admin panel lives under a single URL segment. By default this is `/admin`. You can rename it to a value of your choosing — this renamed segment is called the **Hidden Admin Path**.

| Term | URL | Description |
| --- | --- | --- |
| Default Path | `/admin` | The system default, the same on every install. |
| Hidden Admin Path | `{hidden_admin_path}` | Your renamed segment, overriding the system default. |

That segment is also the **login screen**: visit it while signed out and you'll be asked to sign in.

**Why rename it.** `/admin` is a universal, guessable entry point — the first thing an automated brute-force or credential-stuffing tool will try. Moving the panel to an uncommon segment (for example `/backend`, `/controlpanel`, or anything meaningful to you) doesn't make the login *itself* stronger, but it removes the obvious target: a bot that doesn't know your path can't hammer a login page it can't find. It lowers your exposure rather than eliminating risk, so treat it as one layer, not a substitute for a strong password. **Choose something meaningful to you but uncommon and not easily guessable.**

There are two ways to set it, and a way to reload after editing the file.

### 1 — First choice — change it in Settings

The normal way. Go to [Settings](/admin/settings) → **Admin URL**, type the new path segment, and save. The panel moves immediately and you'll be redirected to the new address.

- Use letters, numbers, hyphens and underscores only.
- Reserved words (`logout`, `api`, `category`, `reset_app`, `home`, …) are rejected.
- **Bookmark the new URL** as soon as you save — the old one stops working.

This value is stored in the database. It applies unless a `config.txt` override is present (next section), which always wins.

### 2 — Override — `config.txt`

A file-based override that **always wins over the Settings value**. This is the safety hatch: if you ever forget your custom admin path and lock yourself out, you can set it here without touching the database or the code.

Create a file at:

```
/App_Data/config.txt
```

and add a single line naming the path segment:

```
admin_url=backend
```

That puts the panel at `/backend`. To put it back to the default, use:

```
admin_url=admin
```

- Lines starting with `#` or `;` are comments. The format is `key=value`.
- While this line is present, the **Admin URL field in Settings becomes read-only** — the file is in charge. Remove the line (or the file) to hand control back to Settings.
- The same character and reserved-word rules apply; an invalid value is ignored and the system falls back to Settings, then to `admin`.

There is a ready-made template to copy from: `/App_Data/config.txt.example`.

#### Other `config.txt` keys

The same file can carry a `dev_mode` line, which is the file-based override for **Dev Mode** (the developer convenience that auto-logs the first admin so admin and API requests work without signing in):

```
dev_mode=true
```

- Accepts `true`/`1`/`on`/`yes` or `false`/`0`/`off`/`no`.
- Like `admin_url`, the file value **wins over the Settings toggle** and is read once at start-up / on `/reset_app`. When present, the Dev Mode switch in Settings becomes informational only.
- This exists so a developer can flip Dev Mode from a file without touching the database — handy for local work and automated tooling.

> ⚠️ **Dev Mode disables the login wall.** With it on, anyone reaching an admin or API URL is treated as the admin, with no password. Only ever set `dev_mode=true` on a local/development machine, and remove it before exposing the site. The default — no line, and the Settings toggle off — keeps the login required.

### 3 — Loading `config.txt` — `/reset_app`

The file is read **once**, so editing it doesn't take effect on its own. There are two ways it gets loaded:

- **Automatically** when the web app (or the server) restarts — the file is read on start-up.
- **On demand**, without a restart, by visiting [`/reset_app`](/reset_app) in your browser. It re-reads `config.txt` and reports where the admin panel now lives.

So the recovery flow for a forgotten admin path is:

1. Create or edit `/App_Data/config.txt` with `admin_url=admin` (or any path you like).
2. Open `/reset_app` — or just restart the app.
3. Go to the path it reports and sign in.

Tip: `/reset_app` always works regardless of the current admin path, so it's safe to rely on even when you're locked out of the panel.

### Order of precedence

When the site decides where the admin panel lives, it checks, in order:

| # | Source | Wins when… |
| --- | --- | --- |
| 1 | `/App_Data/config.txt` → `admin_url=` | the file exists and the value is valid |
| 2 | Settings → Admin URL (database) | no file override, and a value was saved |
| 3 | Built-in default `admin` | neither of the above is set |

The current Hidden Admin Path for this install is shown in the **This install** panel at the top of this page.

## Resetting a forgotten admin login

If you forget the admin **username or password**, reset them with a one-shot file. This is separate from `config.txt` and does not leave any credentials lying around.

1. Create a file at `/App_Data/reset_admin.txt` with two lines:
   ```
   admin_username=youradmin
   admin_password=your-new-password
   ```
2. Restart the app, or visit [`/reset_app`](/reset_app).
3. The admin login is reset to those values, and the file is **automatically consumed** — deleted if possible, otherwise blanked — so the password never lingers on disk.
4. Sign in with the new credentials, then change the password again from the admin panel if you like.

Notes:

> - Username must be at least 3 characters; password at least 6.
> - The reset is only applied *after* the file has been safely removed or blanked — so the same file can never be replayed.
> - Unlike the old approach, nothing is stored as a standing password in `config.txt`; the new password is hashed into the database like any other.

There is a ready-made template to copy from: `/App_Data/reset_admin.txt.example`.
