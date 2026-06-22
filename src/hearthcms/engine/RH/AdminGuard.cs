using System.Collections.Generic;
using System.Data.SQLite;

namespace System.engine.RH
{
    public static class AdminGuard
    {
        // Returns true if request should continue, false if it was redirected/handled.
        public static bool RequireLogin()
        {
            if (Config.IsDevMode && !AppSession.IsLoggedIn)
            {
                // Auto-login as the first admin user for development
                using (var conn = new SQLiteConnection(Config.GetConnString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var s = new SQLiteExpress(cmd);
                        var u = s.GetObject<obUser>("SELECT * FROM users ORDER BY id LIMIT 1;");
                        if (u != null && u.Id > 0) AppSession.LoginUser = u;
                    }
                }
            }

            if (!AppSession.IsLoggedIn)
            {
                // No separate /login route: render the login page in place at
                // whatever admin path was requested (the admin slug is the only
                // entry point). A successful login reloads into the panel.
                LoginPage.HandleRequest();
                return false;
            }
            return true;
        }

        public static bool RequireLoginApi()
        {
            if (Config.IsDevMode && !AppSession.IsLoggedIn)
            {
                using (var conn = new SQLiteConnection(Config.GetConnString()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var s = new SQLiteExpress(cmd);
                        var u = s.GetObject<obUser>("SELECT * FROM users ORDER BY id LIMIT 1;");
                        if (u != null && u.Id > 0) AppSession.LoginUser = u;
                    }
                }
            }
            if (!AppSession.IsLoggedIn)
            {
                ApiHelper.WriteError("Not signed in", 401);
                ApiHelper.EndResponse();
                return false;
            }
            return true;
        }
    }
}
