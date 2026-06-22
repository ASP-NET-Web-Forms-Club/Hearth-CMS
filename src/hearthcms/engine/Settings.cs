namespace System.engine
{
    // ===== Central typed view over the settings table =====
    // A single, discoverable place for every DB-backed setting. Each property is
    // a COMPUTED getter over Db.SettingsCache (the dictionary that Db already
    // loads at startup and reloads after every SaveSetting / ReloadSettings).
    //
    // Because nothing is stored here, this view can never go stale: it always
    // reflects the live cache. There is therefore no "LoadSettings" to call —
    // reads are free and current. Writes continue to go through Db.SaveSetting
    // (the admin save path), which keeps the cache and the database coherent in
    // one place; a couple of thin Set* helpers below just forward to it.
    //
    // Defaults live here, once. Keys that already own a dedicated wrapper keep it
    // and are surfaced here by delegation: DateFormat -> DateDisplay, ActiveTheme
    // -> ThemeManager's builtin slug, AdminSlug/NavMenu stay in their own classes.
    public static class Settings
    {
        // ----- Site identity & branding -----
        public static string SiteName        { get { return Db.GetSetting("site_name", Config.SiteName); } }
        public static string SiteTagline     { get { return Db.GetSetting("site_tagline", Config.SiteTagline); } }
        public static string SiteDescription { get { return Db.GetSetting("site_description", ""); } }
        public static string LogoUrl         { get { return (Db.GetSetting("logo_url", "") ?? "").Trim(); } }
        public static string LogoMode        { get { return (Db.GetSetting("logo_mode", "text") ?? "text").Trim().ToLowerInvariant(); } }
        public static string ThemeColor      { get { return (Db.GetSetting("theme_color", "") ?? "").Trim(); } }
        public static string OgImageUrl      { get { return (Db.GetSetting("og_image_url", "") ?? "").Trim(); } }
        public static string FaviconSourceUrl { get { return (Db.GetSetting("favicon_source_url", "") ?? "").Trim(); } }

        // Favicon cache-buster: at least 1.
        public static int FaviconVersion
        {
            get
            {
                int v;
                if (!int.TryParse(Db.GetSetting("favicon_version", "1"), out v) || v < 1) v = 1;
                return v;
            }
        }

        // ----- Footer -----
        public static string FooterText  { get { return Db.GetSetting("footer_text", ""); } }
        public static int    FooterColCount { get { return GetInt("footer_col_count", 0); } }
        public static string FooterCol1  { get { return FooterColumn(1); } }
        public static string FooterCol2  { get { return FooterColumn(2); } }
        public static string FooterCol3  { get { return FooterColumn(3); } }
        public static string FooterCol4  { get { return FooterColumn(4); } }

        // Footer column markdown by 1-based index (1..4). Handy for the loops in
        // the layout/theme code (footer_col_1 .. footer_col_4).
        public static string FooterColumn(int i)
        {
            return (Db.GetSetting("footer_col_" + i, "") ?? "").Trim();
        }

        // ----- Posts per page -----
        public static int HomePostCount          { get { return GetInt("home_post_count", 6); } }
        public static int LatestPostCount        { get { return GetInt("latest_post_count", 6); } }
        public static int CategoriesPostCount    { get { return GetInt("categories_post_count", 6); } }
        public static int CategoryPostCount      { get { return GetInt("category_post_count", 6); } }
        public static int ArticleSidebarPostCount { get { return GetInt("article_sidebar_post_count", 6); } }

        // ----- Page content caching -----
        // Defaults match the install seed: RAM on, File off, 250 MB budget.
        public static bool CacheRamEnabled  { get { return Db.GetSetting("cache_ram_enabled", "1") == "1"; } }
        public static bool CacheFileEnabled { get { return Db.GetSetting("cache_file_enabled", "0") == "1"; } }
        public static int  CacheRamMaxMb    { get { return GetInt("cache_ram_max_mb", 250); } }

        // ----- Home page -----
        public static string HomePageMode { get { return Db.GetSetting("home_page_mode", "0"); } }
        public static int    HomePageId   { get { return GetInt("home_page_id", 0); } }

        // ----- Active theme ----- (validated + defaulted by ThemeManager)
        public static string ActiveTheme { get { return ThemeManager.GetActiveSlug(); } }

        // ----- Date display ----- (validated + defaulted by DateDisplay)
        public static string DateFormat { get { return DateDisplay.CurrentFormat; } }

        // ===== Write passthroughs =====
        // The single write path remains Db.SaveSetting (cache + DB stay coherent).
        // These exist only so callers can stay in "Settings." land when they want.
        public static void Set(string key, string value) { Db.SaveSetting(key, value); }
        public static void Set(string key, int value)    { Db.SaveSetting(key, value.ToString()); }
        public static void Set(string key, bool value)   { Db.SaveSetting(key, value ? "1" : "0"); }

        // ===== Typed helpers =====
        static int GetInt(string key, int fallback)
        {
            int n;
            if (!int.TryParse(Db.GetSetting(key, fallback.ToString()), out n)) n = fallback;
            return n;
        }
    }
}
