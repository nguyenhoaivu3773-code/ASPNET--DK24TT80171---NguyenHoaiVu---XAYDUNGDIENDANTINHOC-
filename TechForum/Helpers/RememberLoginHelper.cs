using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using TechForum.Models;

namespace TechForum.Helpers
{
    public static class RememberLoginHelper
    {
        private const string UserIdCookieName = "TechForum_RememberUserId";
        private const string TokenCookieName = "TechForum_RememberToken";

        public static string GenerateToken()
        {
            byte[] tokenBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }

            return Convert.ToBase64String(tokenBytes);
        }

        public static void SetRememberCookies(HttpResponseBase response, int userId, string token)
        {
            DateTime expires = DateTime.Now.AddDays(30);

            HttpCookie userIdCookie = new HttpCookie(UserIdCookieName, userId.ToString());
            userIdCookie.Expires = expires;
            userIdCookie.HttpOnly = true;

            HttpCookie tokenCookie = new HttpCookie(TokenCookieName, token);
            tokenCookie.Expires = expires;
            tokenCookie.HttpOnly = true;

            response.Cookies.Add(userIdCookie);
            response.Cookies.Add(tokenCookie);
        }

        public static void ClearRememberCookies(HttpResponseBase response)
        {
            HttpCookie userIdCookie = new HttpCookie(UserIdCookieName);
            userIdCookie.Expires = DateTime.Now.AddDays(-1);

            HttpCookie tokenCookie = new HttpCookie(TokenCookieName);
            tokenCookie.Expires = DateTime.Now.AddDays(-1);

            response.Cookies.Add(userIdCookie);
            response.Cookies.Add(tokenCookie);
        }

        public static void RestoreLoginFromCookie(HttpContextBase context)
        {
            if (context == null || context.Session == null)
            {
                return;
            }

            if (context.Session["UserId"] != null)
            {
                return;
            }

            HttpCookie userIdCookie = context.Request.Cookies[UserIdCookieName];
            HttpCookie tokenCookie = context.Request.Cookies[TokenCookieName];

            if (userIdCookie == null || tokenCookie == null)
            {
                return;
            }

            int userId;

            if (!int.TryParse(userIdCookie.Value, out userId))
            {
                return;
            }

            string token = tokenCookie.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            string tokenHash = PasswordHelper.HashPassword(token);

            using (TechForumDbContext db = new TechForumDbContext())
            {
                User user = db.Users.FirstOrDefault(u =>
                    u.Id == userId &&
                    u.RememberTokenHash == tokenHash &&
                    u.RememberTokenExpiry != null &&
                    u.RememberTokenExpiry > DateTime.Now &&
                    u.IsActive == true
                );

                if (user == null)
                {
                    ClearRememberCookies(context.Response);
                    return;
                }

                context.Session["UserId"] = user.Id;
                context.Session["FullName"] = user.FullName;
                context.Session["Email"] = user.Email;
                context.Session["Role"] = user.Role;
                context.Session["Avatar"] = user.Avatar;
            }
        }
    }
}