using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechForum.Models;

namespace TechForum.Controllers
{
    public class TopicController : BaseController
    {
        private TechForumDbContext db = new TechForumDbContext();

        [HttpGet]
        public ActionResult Index(int? tagId, string keyword, string sort = "newest")
        {
            var query = db.Topics
                .Include("User")
                .Include("TopicTags.Tag")
                .Where(t => t.IsDeleted == false);

            if (tagId.HasValue)
            {
                query = query.Where(t => t.TopicTags.Any(tt => tt.TagId == tagId.Value));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(t =>
                    t.Title.Contains(keyword) ||
                    t.Content.Contains(keyword)
                );
            }

            if (sort != "newest" && sort != "most-voted" && sort != "most-discussed")
            {
                sort = "newest";
            }

            var topicList = query.ToList();

            var topics = topicList
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

            if (sort == "most-voted")
            {
                topics = topics
                    .OrderByDescending(t => t.VoteCount)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
            }
            else if (sort == "most-discussed")
            {
                topics = topics
                    .OrderByDescending(t => t.CommentCount)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
            }
            else
            {
                topics = topics
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }

            var topUsersByVotes = db.Users
                .Where(u => u.IsActive == true)
                .ToList()
                .Select(u => new TopUserVoteVM
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Avatar = string.IsNullOrEmpty(u.Avatar)
                        ? "/Uploads/Avatars/default-avatar.png"
                        : u.Avatar,

                    TotalVotes = db.Topics
                        .Where(t =>
                            t.UserId == u.Id &&
                            t.IsDeleted == false)
                        .Select(t => t.VoteCount)
                        .DefaultIfEmpty(0)
                        .Sum(),

                    TotalTopics = db.Topics.Count(t =>
                        t.UserId == u.Id &&
                        t.IsDeleted == false)
                })
                .Where(u => u.TotalVotes > 0)
                .OrderByDescending(u => u.TotalVotes)
                .ThenByDescending(u => u.TotalTopics)
                .Take(10)
                .ToList();

            TopicIndexVM model = new TopicIndexVM
            {
                Topics = topics,
                Tags = db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToList(),
                SelectedTagId = tagId,
                Keyword = keyword,
                TopUsersByVotes = topUsersByVotes,
                Sort = sort
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            TopicCreateVM model = new TopicCreateVM
            {
                AvailableTags = db.Tags
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TopicCreateVM model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            model.AvailableTags = db.Tags
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SelectedTagIds == null || model.SelectedTagIds.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 thẻ cho chủ đề");
                return View(model);
            }

            if (model.SelectedTagIds.Count > 10)
            {
                ModelState.AddModelError("", "Mỗi chủ đề chỉ được chọn tối đa 10 thẻ");
                return View(model);
            }

            var validTagIds = db.Tags
                .Where(t => t.IsActive)
                .Select(t => t.Id)
                .ToList();

            bool hasInvalidTag = model.SelectedTagIds.Any(id => !validTagIds.Contains(id));

            if (hasInvalidTag)
            {
                ModelState.AddModelError("", "Danh sách thẻ không hợp lệ");
                return View(model);
            }

            var uploadedFiles = Request.Files;
            int imageCount = 0;

            for (int i = 0; i < uploadedFiles.Count; i++)
            {
                HttpPostedFileBase file = uploadedFiles[i];

                if (file != null && file.ContentLength > 0)
                {
                    imageCount++;
                }
            }

            if (imageCount > 3)
            {
                ModelState.AddModelError("", "Mỗi chủ đề chỉ được upload tối đa 3 ảnh");
                return View(model);
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            Topic topic = new Topic
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                UserId = userId,
                ViewCount = 0,
                VoteCount = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                IsDeleted = false
            };

            db.Topics.Add(topic);
            db.SaveChanges();

            foreach (int tagIdValue in model.SelectedTagIds.Distinct())
            {
                TopicTag topicTag = new TopicTag
                {
                    TopicId = topic.Id,
                    TagId = tagIdValue
                };

                db.TopicTags.Add(topicTag);
            }

            if (imageCount > 0)
            {
                string folderPath = Server.MapPath("~/Uploads/Topics/");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                int maxSize = 3 * 1024 * 1024;

                for (int i = 0; i < uploadedFiles.Count; i++)
                {
                    HttpPostedFileBase file = uploadedFiles[i];

                    if (file == null || file.ContentLength <= 0)
                    {
                        continue;
                    }

                    string extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Ảnh chỉ được dùng định dạng .jpg, .jpeg, .png, .gif hoặc .webp");
                        return View(model);
                    }

                    if (file.ContentLength > maxSize)
                    {
                        ModelState.AddModelError("", "Mỗi ảnh không được vượt quá 3MB");
                        return View(model);
                    }

                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + extension;
                    string savePath = Path.Combine(folderPath, fileName);

                    file.SaveAs(savePath);

                    TopicImage topicImage = new TopicImage
                    {
                        TopicId = topic.Id,
                        ImagePath = "/Uploads/Topics/" + fileName,
                        CreatedAt = DateTime.Now
                    };

                    db.TopicImages.Add(topicImage);
                }
            }

            db.SaveChanges();

            TempData["SuccessMessage"] = "Tạo chủ đề thành công.";

            return RedirectToAction("Details", new { id = topic.Id });
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            Topic topic = db.Topics
                .Include("User")
                .Include("TopicImages")
                .Include("TopicTags.Tag")
                .FirstOrDefault(t => t.Id == id);

            if (topic == null)
            {
                return HttpNotFound();
            }

            bool isAdmin = Session["Role"] != null && Session["Role"].ToString() == "Admin";

            if (topic.IsDeleted && !isAdmin)
            {
                return HttpNotFound();
            }

            topic.ViewCount += 1;
            db.SaveChanges();

            var parentComments = db.TopicComments
                .Include("User")
                .Include("Replies.User")
                .Where(c =>
                    c.TopicId == id &&
                    c.ParentCommentId == null &&
                    c.Level == 1 &&
                    c.IsDeleted == false
                )
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            foreach (var comment in parentComments)
            {
                comment.Replies = comment.Replies
                    .Where(r => r.IsDeleted == false)
                    .OrderBy(r => r.CreatedAt)
                    .ToList();
            }

            bool hasVoted = false;

            if (Session["UserId"] != null)
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                hasVoted = db.TopicVotes.Any(v =>
                    v.TopicId == id &&
                    v.UserId == userId
                );
            }

            var authorTopVotedTopics = db.Topics
                .Include("User")
                .Include("TopicTags.Tag")
                .Where(t =>
                    t.UserId == topic.UserId &&
                    t.Id != topic.Id &&
                    t.IsDeleted == false)
                .OrderByDescending(t => t.VoteCount)
                .ThenByDescending(t => t.ViewCount)
                .ThenByDescending(t => t.CreatedAt)
                .Take(5)
                .ToList()
                .Select(t => new TopicIndexItemVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content,
                    UserFullName = t.User != null ? t.User.FullName : "Ẩn danh",
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

            TopicDetailsVM model = new TopicDetailsVM
            {
                Topic = topic,
                ParentComments = parentComments,
                HasVoted = hasVoted,
                AuthorTopVotedTopics = authorTopVotedTopics
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int topicId, string content, int? parentCommentId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction("Details", new { id = topicId });
            }

            Topic topic = db.Topics.FirstOrDefault(t => t.Id == topicId && t.IsDeleted == false);

            if (topic == null)
            {
                return HttpNotFound();
            }

            int level = 1;

            if (parentCommentId.HasValue)
            {
                TopicComment parent = db.TopicComments.FirstOrDefault(c =>
                    c.Id == parentCommentId.Value &&
                    c.TopicId == topicId &&
                    c.IsDeleted == false
                );

                if (parent == null)
                {
                    return HttpNotFound();
                }

                if (parent.Level >= 2)
                {
                    TempData["ErrorMessage"] = "Chỉ cho phép bình luận tối đa 2 cấp.";
                    return RedirectToAction("Details", new { id = topicId });
                }

                level = 2;
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            TopicComment comment = new TopicComment
            {
                TopicId = topicId,
                UserId = userId,
                Content = content.Trim(),
                ParentCommentId = parentCommentId,
                Level = level,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            db.TopicComments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = topicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vote(int topicId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            Topic topic = db.Topics.FirstOrDefault(t => t.Id == topicId && t.IsDeleted == false);

            if (topic == null)
            {
                return HttpNotFound();
            }

            bool hasVoted = db.TopicVotes.Any(v =>
                v.TopicId == topicId &&
                v.UserId == userId
            );

            if (hasVoted)
            {
                TempData["ErrorMessage"] = "Bạn đã vote chủ đề này rồi.";
                return RedirectToAction("Details", new { id = topicId });
            }

            TopicVote vote = new TopicVote
            {
                TopicId = topicId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            db.TopicVotes.Add(vote);

            topic.VoteCount += 1;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = topicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Report(int topicId, string reason)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do báo cáo.";
                return RedirectToAction("Details", new { id = topicId });
            }

            if (reason.Trim().Length > 500)
            {
                TempData["ErrorMessage"] = "Lý do báo cáo không được vượt quá 500 ký tự.";
                return RedirectToAction("Details", new { id = topicId });
            }

            Topic topic = db.Topics.FirstOrDefault(t =>
                t.Id == topicId &&
                t.IsDeleted == false
            );

            if (topic == null)
            {
                return HttpNotFound();
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            if (topic.UserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể báo cáo chủ đề do chính mình tạo.";
                return RedirectToAction("Details", new { id = topicId });
            }

            bool hasPendingReport = db.TopicReports.Any(r =>
                r.TopicId == topicId &&
                r.ReporterUserId == userId &&
                r.Status == "Pending"
            );

            if (hasPendingReport)
            {
                TempData["ErrorMessage"] = "Bạn đã báo cáo chủ đề này rồi. Vui lòng chờ admin xử lý.";
                return RedirectToAction("Details", new { id = topicId });
            }

            TopicReport report = new TopicReport
            {
                TopicId = topicId,
                ReporterUserId = userId,
                Reason = reason.Trim(),
                Status = "Pending",
                AdminNote = null,
                CreatedAt = DateTime.Now,
                HandledAt = null,
                HandledByUserId = null
            };

            db.TopicReports.Add(report);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Báo cáo của bạn đã được gửi. Admin sẽ xem xét và xử lý.";

            return RedirectToAction("Details", new { id = topicId });
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