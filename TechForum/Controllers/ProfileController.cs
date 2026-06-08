using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechForum.Models;

namespace TechForum.Controllers
{
    public class ProfileController : BaseController
    {
        private TechForumDbContext db = new TechForumDbContext();

        // GET: /Profile/Index?filter=mine
        [HttpGet]
        public ActionResult Index(string filter = "mine")
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            User user = db.Users.FirstOrDefault(u =>
                u.Id == userId &&
                u.IsActive == true
            );

            if (user == null)
            {
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            if (filter != "mine" && filter != "unanswered" && filter != "voted")
            {
                filter = "mine";
            }

            var topicQuery = db.Topics
                .Include("User")
                .Include("TopicTags.Tag")
                .Where(t => t.IsDeleted == false);

            if (filter == "mine")
            {
                topicQuery = topicQuery.Where(t => t.UserId == userId);
            }
            else if (filter == "unanswered")
            {
                topicQuery = topicQuery.Where(t =>
                    t.UserId == userId &&
                    !db.TopicComments.Any(c => c.TopicId == t.Id && c.IsDeleted == false)
                );
            }
            else if (filter == "voted")
            {
                topicQuery = topicQuery.Where(t =>
                    db.TopicVotes.Any(v =>
                        v.TopicId == t.Id &&
                        v.UserId == userId
                    )
                );
            }

            var topics = topicQuery
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .Select(t => new TopicIndexItemVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content,
                    UserId = t.UserId,
                    UserFullName = t.User != null ? t.User.FullName : "Ẩn danh",
                    UserAvatar = t.User != null && !string.IsNullOrEmpty(t.User.Avatar)
                        ? t.User.Avatar
                        : "/Uploads/Avatars/user.png",
                    CreatedAt = t.CreatedAt,
                    ViewCount = t.ViewCount,
                    VoteCount = t.VoteCount,
                    CommentCount = db.TopicComments.Count(c =>
                        c.TopicId == t.Id &&
                        c.IsDeleted == false),
                    Tags = t.TopicTags != null
                        ? t.TopicTags.Select(x => x.Tag).ToList()
                        : new System.Collections.Generic.List<Tag>()
                })
                .ToList();

            ProfileVM model = new ProfileVM
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Avatar = string.IsNullOrEmpty(user.Avatar)
                    ? "/Uploads/Avatars/default-avatar.png"
                    : user.Avatar,

                TotalTopics = db.Topics.Count(t =>
                    t.UserId == userId &&
                    t.IsDeleted == false),

                TotalVotes = db.Topics
                    .Where(t =>
                        t.UserId == userId &&
                        t.IsDeleted == false)
                    .Select(t => t.VoteCount)
                    .DefaultIfEmpty(0)
                    .Sum(),

                TotalComments = db.TopicComments.Count(c =>
                    c.UserId == userId &&
                    c.IsDeleted == false),

                Filter = filter,
                Topics = topics
            };

            return View(model);
        }

        // POST: /Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ProfileVM model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            User user = db.Users.FirstOrDefault(u =>
                u.Id == userId &&
                u.IsActive == true
            );

            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                TempData["ErrorMessage"] = "Họ tên không được để trống.";
                return RedirectToAction("Index");
            }

            if (model.FullName.Trim().Length > 100)
            {
                TempData["ErrorMessage"] = "Họ tên không được vượt quá 100 ký tự.";
                return RedirectToAction("Index");
            }

            user.FullName = model.FullName.Trim();

            if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(model.AvatarFile.FileName).ToLower();

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Ảnh đại diện chỉ cho phép .jpg, .jpeg, .png, .gif hoặc .webp.";
                    return RedirectToAction("Index");
                }

                int maxSize = 2 * 1024 * 1024;

                if (model.AvatarFile.ContentLength > maxSize)
                {
                    TempData["ErrorMessage"] = "Ảnh đại diện không được vượt quá 2MB.";
                    return RedirectToAction("Index");
                }

                string folderPath = Server.MapPath("~/Uploads/Avatars/");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + extension;
                string savePath = Path.Combine(folderPath, fileName);

                model.AvatarFile.SaveAs(savePath);

                user.Avatar = "/Uploads/Avatars/" + fileName;
            }

            db.SaveChanges();

            Session["FullName"] = user.FullName;
            Session["Avatar"] = user.Avatar;

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";

            return RedirectToAction("Index");
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