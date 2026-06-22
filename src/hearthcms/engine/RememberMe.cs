using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace System.engine
{
    // Persistent "remember me" login via a long-lived cookie backed by SQLite.
    // Cookie format: "<selector>.<validator>" (both hex). The selector indexes the
    // row; the validator is compared constant-time against its SHA-256 hash. A DB
    // read alone does not yield a usable token, and stolen cookies can be revoked
    // by deleting the row.
    public static class RememberMe
    {
        public const string CookieName = "rmt";
        const int SelectorBytes = 12;
        const int ValidatorBytes = 32;
        const int DefaultDays = 30;

        public static void Issue(int userId)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || userId <= 0) return;

            string selector = RandomHex(SelectorBytes);
            byte[] validatorBytes = RandomBytes(ValidatorBytes);
            string validator = ToHex(validatorBytes);
            string validatorHash = Sha256Hex(validatorBytes);
            DateTime expires = DateTime.UtcNow.AddDays(DefaultDays);

            using (var conn = new SQLiteConnection(Config.GetConnString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var s = new SQLiteExpress(cmd);
                    var d = new Dictionary<string, object>();
                    d["selector"] = selector;
                    d["validator_hash"] = validatorHash;
                    d["user_id"] = userId;
                    d["expires_at"] = expires;
                    d["date_created"] = DateTime.UtcNow;
                    s.Insert("user_sessions", d);
                }
            }

            var c = new HttpCookie(CookieName, selector + "." + validator);
            c.HttpOnly = true;
            c.Secure = ctx.Request.IsSecureConnection;
            c.SameSite = SameSiteMode.Lax;
            c.Path = "/";
            c.Expires = expires;
            ctx.Response.Cookies.Set(c);
        }

        // If no active login but a valid remember cookie is present, load the user
        // and populate AppSession.LoginUser. Rotates the validator on success.
        public static void TryRestore()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null) return;
            if (SessionStore.HasActiveLogin()) return;

            HttpCookie cookie = ctx.Request.Cookies[CookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return;

            string[] parts = cookie.Value.Split('.');
            if (parts.Length != 2) { ClearCookie(); return; }
            string selector = parts[0];
            string validator = parts[1];
            if (string.IsNullOrEmpty(selector) || string.IsNullOrEmpty(validator))
            {
                ClearCookie(); return;
            }

            obUser user = null;

            using (var conn = new SQLiteConnection(Config.GetConnString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var s = new SQLiteExpress(cmd);
                    var p = new Dictionary<string, object> { { "@s", selector } };
                    var dt = s.Select(
                        "SELECT id, validator_hash, user_id, expires_at FROM user_sessions WHERE selector=@s LIMIT 1;", p);
                    if (dt == null || dt.Rows.Count == 0) { ClearCookie(); return; }
                    var r = dt.Rows[0];
                    int rowId = Convert.ToInt32(r["id"]);
                    string storedHash = r["validator_hash"] as string ?? "";
                    int userId = Convert.ToInt32(r["user_id"]);
                    DateTime expires = Convert.ToDateTime(r["expires_at"]);

                    if (DateTime.UtcNow > expires)
                    {
                        var pp = new Dictionary<string, object> { { "@i", rowId } };
                        s.Execute("DELETE FROM user_sessions WHERE id=@i;", pp);
                        ClearCookie();
                        return;
                    }

                    byte[] presented;
                    try { presented = FromHex(validator); }
                    catch { ClearCookie(); return; }
                    string presentedHash = Sha256Hex(presented);
                    if (!ConstantTimeEquals(presentedHash, storedHash))
                    {
                        // Mismatch likely means a stolen-cookie attempt; nuke the row.
                        var pp = new Dictionary<string, object> { { "@i", rowId } };
                        s.Execute("DELETE FROM user_sessions WHERE id=@i;", pp);
                        ClearCookie();
                        return;
                    }

                    var pu = new Dictionary<string, object> { { "@i", userId } };
                    user = s.GetObject<obUser>("SELECT * FROM users WHERE id=@i LIMIT 1;", pu);
                    if (user == null || user.Id == 0)
                    {
                        var pp = new Dictionary<string, object> { { "@i", rowId } };
                        s.Execute("DELETE FROM user_sessions WHERE id=@i;", pp);
                        ClearCookie();
                        return;
                    }

                    byte[] newValidatorBytes = RandomBytes(ValidatorBytes);
                    string newValidator = ToHex(newValidatorBytes);
                    string newHash = Sha256Hex(newValidatorBytes);
                    DateTime newExpires = DateTime.UtcNow.AddDays(DefaultDays);
                    var pr = new Dictionary<string, object>
                    {
                        { "@h", newHash },
                        { "@e", newExpires },
                        { "@i", rowId }
                    };
                    s.Execute("UPDATE user_sessions SET validator_hash=@h, expires_at=@e WHERE id=@i;", pr);

                    var c = new HttpCookie(CookieName, selector + "." + newValidator);
                    c.HttpOnly = true;
                    c.Secure = ctx.Request.IsSecureConnection;
                    c.SameSite = SameSiteMode.Lax;
                    c.Path = "/";
                    c.Expires = newExpires;
                    ctx.Response.Cookies.Set(c);
                }
            }

            AppSession.LoginUser = user;
        }

        public static void RevokeCurrent()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null) return;
            HttpCookie cookie = ctx.Request.Cookies[CookieName];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string[] parts = cookie.Value.Split('.');
                if (parts.Length == 2 && !string.IsNullOrEmpty(parts[0]))
                {
                    try
                    {
                        using (var conn = new SQLiteConnection(Config.GetConnString()))
                        {
                            conn.Open();
                            using (var cmd = conn.CreateCommand())
                            {
                                var s = new SQLiteExpress(cmd);
                                var p = new Dictionary<string, object> { { "@s", parts[0] } };
                                s.Execute("DELETE FROM user_sessions WHERE selector=@s;", p);
                            }
                        }
                    }
                    catch { }
                }
            }
            ClearCookie();
        }

        static void ClearCookie()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null) return;
            var c = new HttpCookie(CookieName, "");
            c.Expires = DateTime.UtcNow.AddDays(-1);
            c.Path = "/";
            ctx.Response.Cookies.Set(c);
        }

        static byte[] RandomBytes(int n)
        {
            byte[] buf = new byte[n];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(buf);
            return buf;
        }

        static string RandomHex(int n) { return ToHex(RandomBytes(n)); }

        static string ToHex(byte[] b)
        {
            var sb = new StringBuilder(b.Length * 2);
            for (int i = 0; i < b.Length; i++) sb.Append(b[i].ToString("x2"));
            return sb.ToString();
        }

        static byte[] FromHex(string hex)
        {
            if (hex == null) hex = "";
            if ((hex.Length & 1) != 0) throw new FormatException("odd hex length");
            byte[] b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }

        static string Sha256Hex(byte[] data)
        {
            using (var sha = SHA256.Create()) return ToHex(sha.ComputeHash(data));
        }

        static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
