using System;
using System.Linq;
using System.Web.Mvc;
using TechForum.Models;

namespace TechForum.Controllers
{
    public class AdminController : BaseController
    {
        private TechForumDbContext db = new TechForumDbContext();
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            AdminDashboardVM model = new AdminDashboardVM
            {
                TotalUsers = db.Users.Count(u => u.IsActive == true),

                TotalTopics = db.Topics.Count(t => t.IsDeleted == false),

                TotalTags = db.Tags.Count(t => t.IsActive == true),

                PendingReports = db.TopicReports.Count(r => r.Status == "Pending")
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult TopicReports()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            var reports = db.TopicReports
                .Include("Topic")
                .Include("ReporterUser")
                .Include("HandledByUser")
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(reports);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleTopicReport(int reportId, string status, string adminNote, bool hideTopic = false)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            TopicReport report = db.TopicReports
                .Include("Topic")
                .FirstOrDefault(r => r.Id == reportId);

            if (report == null)
            {
                return HttpNotFound();
            }

            if (status != "Approved" && status != "Rejected")
            {
                TempData["ErrorMessage"] = "Trạng thái xử lý không hợp lệ.";
                return RedirectToAction("TopicReports");
            }

            report.Status = status;
            report.AdminNote = adminNote;
            report.HandledAt = DateTime.Now;
            report.HandledByUserId = Convert.ToInt32(Session["UserId"]);

            if (hideTopic && report.Topic != null)
            {
                report.Topic.IsDeleted = true;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã xử lý báo cáo.";

            return RedirectToAction("TopicReports");
        }

        [HttpGet]
        public ActionResult Users()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            var users = db.Users
                .OrderByDescending(u => u.Id)
                .ToList()
                .Select(u => new AdminUserItemVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    TotalTopics = db.Topics.Count(t =>
                        t.UserId == u.Id &&
                        t.IsDeleted == false)
                })
                .ToList();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleUserStatus(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            int currentAdminId = Convert.ToInt32(Session["UserId"]);

            if (id == currentAdminId)
            {
                TempData["ErrorMessage"] = "Bạn không thể khóa chính tài khoản admin đang đăng nhập.";
                return RedirectToAction("Users");
            }

            User user = db.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.Role == "Admin")
            {
                TempData["ErrorMessage"] = "Không thể khóa tài khoản Admin khác.";
                return RedirectToAction("Users");
            }

            user.IsActive = !user.IsActive;

            db.SaveChanges();

            if (user.IsActive)
            {
                TempData["SuccessMessage"] = "Đã mở khóa tài khoản người dùng.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã khóa tài khoản người dùng.";
            }

            return RedirectToAction("Users");
        }

        private string MakeShortContent(string content, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            content = content.Trim();

            if (content.Length <= maxLength)
            {
                return content;
            }

            return content.Substring(0, maxLength) + ".....";
        }

        [HttpGet]
        public ActionResult Topics()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            var topics = db.Topics
                .Include("User")
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .Select(t => new AdminTopicItemVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    ShortContent = MakeShortContent(t.Content, 90),
                    UserFullName = t.User != null ? t.User.FullName : "Không rõ",
                    CreatedAt = t.CreatedAt,
                    ViewCount = t.ViewCount,
                    VoteCount = t.VoteCount,
                    CommentCount = db.TopicComments.Count(c =>
                        c.TopicId == t.Id &&
                        c.IsDeleted == false),
                    IsDeleted = t.IsDeleted
                })
                .ToList();

            return View(topics);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTopic(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            Topic topic = db.Topics.FirstOrDefault(t => t.Id == id);

            if (topic == null)
            {
                return HttpNotFound();
            }

            // Xóa báo cáo liên quan
            var reports = db.TopicReports
                .Where(r => r.TopicId == id)
                .ToList();

            foreach (var report in reports)
            {
                db.TopicReports.Remove(report);
            }

            // Xóa vote liên quan
            var votes = db.TopicVotes
                .Where(v => v.TopicId == id)
                .ToList();

            foreach (var vote in votes)
            {
                db.TopicVotes.Remove(vote);
            }

            // Xóa thẻ liên kết
            var topicTags = db.TopicTags
                .Where(tt => tt.TopicId == id)
                .ToList();

            foreach (var topicTag in topicTags)
            {
                db.TopicTags.Remove(topicTag);
            }

            // Xóa bình luận cấp 2 trước
            var childComments = db.TopicComments
                .Where(c =>
                    c.TopicId == id &&
                    c.ParentCommentId != null)
                .ToList();

            foreach (var comment in childComments)
            {
                db.TopicComments.Remove(comment);
            }

            // Xóa bình luận cấp 1 sau
            var parentComments = db.TopicComments
                .Where(c =>
                    c.TopicId == id &&
                    c.ParentCommentId == null)
                .ToList();

            foreach (var comment in parentComments)
            {
                db.TopicComments.Remove(comment);
            }

            // Xóa ảnh trong database và file vật lý
            var images = db.TopicImages
                .Where(i => i.TopicId == id)
                .ToList();

            foreach (var image in images)
            {
                if (!string.IsNullOrEmpty(image.ImagePath))
                {
                    string physicalPath = Server.MapPath("~" + image.ImagePath);

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                db.TopicImages.Remove(image);
            }

            // Xóa chủ đề
            db.Topics.Remove(topic);

            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã xóa chủ đề khỏi hệ thống.";

            return RedirectToAction("Topics");
        }

        [HttpGet]
        public ActionResult Tags()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            AdminTagVM model = new AdminTagVM
            {
                Tags = db.Tags
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList()
                    .Select(t => new AdminTagItemVM
                    {
                        Id = t.Id,
                        Name = t.Name,
                        CreatedAt = t.CreatedAt,
                        IsActive = t.IsActive,
                        TopicCount = db.TopicTags.Count(tt => tt.TagId == t.Id)
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTag(string newTagName)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            if (string.IsNullOrWhiteSpace(newTagName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên thẻ.";
                return RedirectToAction("Tags");
            }

            string tagName = newTagName.Trim();

            if (tagName.Length > 50)
            {
                TempData["ErrorMessage"] = "Tên thẻ không được vượt quá 50 ký tự.";
                return RedirectToAction("Tags");
            }

            bool tagExists = db.Tags.Any(t => t.Name.ToLower() == tagName.ToLower());

            if (tagExists)
            {
                TempData["ErrorMessage"] = "Tên thẻ này đã tồn tại.";
                return RedirectToAction("Tags");
            }

            Tag tag = new Tag
            {
                Name = tagName,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            db.Tags.Add(tag);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã thêm thẻ mới thành công.";

            return RedirectToAction("Tags");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleTagStatus(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Topic");
            }

            Tag tag = db.Tags.FirstOrDefault(t => t.Id == id);

            if (tag == null)
            {
                return HttpNotFound();
            }

            tag.IsActive = !tag.IsActive;

            db.SaveChanges();

            if (tag.IsActive)
            {
                TempData["SuccessMessage"] = "Đã bật lại thẻ.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã ẩn thẻ.";
            }

            return RedirectToAction("Tags");
        }
    }
}