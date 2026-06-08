using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using TechForum.Helpers;
using TechForum.Models;

namespace TechForum.Controllers
{
    public class AccountController : BaseController
    {
        private TechForumDbContext db = new TechForumDbContext();

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLower();

            bool emailExists = db.Users.Any(u => u.Email.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(model);
            }

            string avatarPath = "/Uploads/Avatars/user.png";

            if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(model.AvatarFile.FileName).ToLower();

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("AvatarFile", "Chỉ cho phép upload ảnh .jpg, .jpeg, .png hoặc .gif");
                    return View(model);
                }

                int maxSize = 2 * 1024 * 1024;

                if (model.AvatarFile.ContentLength > maxSize)
                {
                    ModelState.AddModelError("AvatarFile", "Ảnh đại diện không được vượt quá 2MB");
                    return View(model);
                }

                string folderPath = Server.MapPath("~/Uploads/Avatars/");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + extension;

                string savePath = Path.Combine(folderPath, fileName);

                model.AvatarFile.SaveAs(savePath);

                avatarPath = "/Uploads/Avatars/" + fileName;
            }

            User user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = "User",
                Avatar = avatarPath,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            db.Users.Add(user);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";

            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            LoginVM model = new LoginVM
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLower();
            string passwordHash = PasswordHelper.HashPassword(model.Password);

            User user = db.Users.FirstOrDefault(u =>
                u.Email.ToLower() == email &&
                u.PasswordHash == passwordHash &&
                u.IsActive == true
            );

            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            Session["UserId"] = user.Id;
            Session["FullName"] = user.FullName;
            Session["Email"] = user.Email;
            Session["Role"] = user.Role;
            Session["Avatar"] = user.Avatar;

            if (model.RememberMe)
            {
                string token = RememberLoginHelper.GenerateToken();
                string tokenHash = PasswordHelper.HashPassword(token);

                user.RememberTokenHash = tokenHash;
                user.RememberTokenExpiry = DateTime.Now.AddDays(30);

                db.SaveChanges();

                RememberLoginHelper.SetRememberCookies(Response, user.Id, token);
            }
            else
            {
                user.RememberTokenHash = null;
                user.RememberTokenExpiry = null;

                db.SaveChanges();

                RememberLoginHelper.ClearRememberCookies(Response);
            }

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Topic");
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            if (Session["UserId"] != null)
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                User user = db.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    user.RememberTokenHash = null;
                    user.RememberTokenExpiry = null;
                    db.SaveChanges();
                }
            }

            RememberLoginHelper.ClearRememberCookies(Response);

            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}