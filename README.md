# Hearth CMS

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/download/dotnet-framework)
[![C#](https://img.shields.io/badge/C%23-7.3-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![SQLite](https://img.shields.io/badge/SQLite-single--file-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Dependencies](https://img.shields.io/badge/NuGet%20deps-2-brightgreen)](#requirements)
[![Status](https://img.shields.io/badge/status-production%20ready-success)]()
[![License](https://img.shields.io/badge/license-MIT-lightgrey)](LICENSE)

A lightweight, self-contained content management system built on ASP.NET (.NET Framework 4.8). All content lives in a single SQLite file — no SQL Server, no MySQL, no external services. Just two NuGet dependencies, and you install it by deploying it.

Hearth is built by using **Pageless ASP.NET Web Forms Architecture** (PAW). Every page is rendered in C# and routed through a single switch in `Global.asax.cs`. No `.aspx` files, no master pages, no server controls, no ViewState, no page lifecycle. The frontend talks to the backend purely through the Fetch API. The world's first web application uses PAW Architecture. This project serves as the testimony of PAW Architecture.

The architecture behind this CMS is documented in a dedicated article series:

- [Introducing ASP.NET Web Forms Pageless Architecture](https://adriancs.com)
- [True Pageless Architecture with Custom Session State](https://adriancs.com/asp-net-web-forms-true-pageless-architecture-with-custom-session-state/)
- [Three Approaches to ASP.NET Web Forms Architecture](https://adriancs.com/three-approaches-to-asp-net-web-forms-architecture/)
- [Comparing Web Architectures in the .NET World](https://adriancs.com/comparing-web-architectures-in-net-world/)

![Screenshot - Theme Hearth](https://raw.githubusercontent.com/ASP-NET-Web-Forms-Club/Hearth-CMS/refs/heads/main/src/hearthcms/assets/themes/hearth-cs/showcase.jpg)
![Screenshot - Theme RiverCove](https://raw.githubusercontent.com/ASP-NET-Web-Forms-Club/Hearth-CMS/refs/heads/main/src/hearthcms/assets/themes/RiverCove/showcase.jpg)
![Screenshot - Theme Corporate](https://raw.githubusercontent.com/ASP-NET-Web-Forms-Club/Hearth-CMS/refs/heads/main/src/hearthcms/assets/themes/Corporate/showcase.jpg)
![Screenshot - Theme Valentine](https://raw.githubusercontent.com/ASP-NET-Web-Forms-Club/Hearth-CMS/refs/heads/main/src/hearthcms/assets/themes/ValentineLight/showcase.jpg)

## Features

- **Posts & pages** — full writing and management workflow, draft/publish, soft-delete trash, slugs served at the site root.
- **Dual content editor** — write in a **WYSIWYG HTML editor** or a **Markdown editor**, per item.
- **Built-in Markdown engine** — a dependency-free, single-pass Markdown to HTML converter.
- **Media library** — upload, browse, and manage assets with automatic image thumbnailing.
- **Categories & navigation** — organise posts and build the site menu in-app.
- **Two theming models** — folder-based HTML themes (editable in the admin) *and* compiled C# themes. Ships with reference themes (Hearth, Broadsheet).
- **Lightning-fast page serving** — two-tier public page cache: an LRU, byte-budgeted **RAM cache** plus an optional **file cache**, shared across all visitors.
- **Automatic favicon generator** — upload one master image; Hearth produces the full favicon set (ICO + PNGs + web manifest).
- **Automatic OG / social meta tags** — Open Graph and Twitter Card tags wired across every page.
- **Automatic sitemap** — live-generated `sitemap.xml` and `robots.txt`.
- **Custom session + Remember Me** — lock-free in-memory session available at `BeginRequest`, persistent across app-pool recycles.
- **Operator escape hatches** — secret admin URL slug, `/reset_app` config reload without restart, one-shot admin-credential reset, and a logged additive schema-migration system.
- **20 HTML themes ship in the box**: 18 HTML themes (Almanac, Cathode, Cipher, Corporate, Diving, DivingDark, Element, Folio, Legal, LegalDark, Mosaic, Riso, RiverCove, Solarpunk, Swiss, Valentine, ValentineLight, Victoria) plus 2 C# themes (Hearth, Broadsheet).

## Requirements

- Windows, **.NET Framework 4.8**
- NuGet packages (restored automatically): `Newtonsoft.Json`, `Stub.System.Data.SQLite.Core.NetFramework`

## Installation and Deployment

- Method 1: Deploy through Windows IIS / Web Hosting
- Method 2: Using Hearth Portable ASP.NET Web Server 

### Method 1: Deploy through Windows IIS / Web Hosting

1. Install Windows built-in IIS Web Server (Available in Windows 10/11 Pro / Windows Server)
2. Add the CMS folder as a website in IIS.

### Method 2: Using Hearth Portable ASP.NET Web Server

1. Download pre-package solution at Release or download it at [Github-Hearth Web Server](https://github.com/ASP-NET-Web-Forms-Club/Hearth-ASPNET-Server).
2. Run the Launcher EXE and start the web server.

### Installation

1. Open it in a browser — the first request routes to the setup page.
2. Enter your site name, admin username/password, and (optionally) a custom admin URL.
3. On submit, Hearth creates and seeds `App_Data/cms.sqlite` (admin user, starter pages, sample posts, default navigation) and locks the installer permanently. "Installed" simply means the database file exists.

## Configuration & recovery

- **Custom admin URL** — set a secret slug so the admin panel isn't at the default path.
- **`/reset_app`** — reloads `App_Data/config.txt` and settings live, no restart needed.
- **`App_Data/reset_admin.txt`** — drop in a one-shot file to safely reset admin credentials.

## Architecture

See [`Pageless-Architecture.txt`](src/hearthcms/Pageless-Architecture.txt) for a full reference on the Pageless model, the two-handler (page + API) pattern, custom session state, and the C# theming system.

## License

MIT © 2026 adriancs2 — see [LICENSE](LICENSE).
