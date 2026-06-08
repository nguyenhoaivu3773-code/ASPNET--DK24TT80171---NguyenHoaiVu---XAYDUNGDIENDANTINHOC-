using System.Linq;
using System.Web.Mvc;
using TechForum.Models;

namespace TechForum.Controllers
{
    public class UserController : BaseController
    {
        private TechForumDbContext db = new TechForumDbContext();

        // GET: /User/Index
        [HttpGet]
        public ActionResult Index(string keyword)
        {
            var users = db.Users
                .Where(u => u.IsActive == true && u.Role != "Admin")
                .OrderBy(u => u.FullName)
                .ToList()
                .Select(u => new UserIndexItemVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    Avatar = string.IsNullOrEmpty(u.Avatar)
                        ? "/Uploads/Avatars/default-avatar.png"
                        : u.Avatar,

                    TotalTopics = db.Topics.Count(t =>
                        t.UserId == u.Id &&
                        t.IsDeleted == false),

                    TotalVotes = db.Topics
                        .Where(t =>
                            t.UserId == u.Id &&
                            t.IsDeleted == false)
                        .Select(t => t.VoteCount)
                        .DefaultIfEmpty(0)
                        .Sum(),

                    TotalComments = db.TopicComments.Count(c =>
                        c.UserId == u.Id &&
                        c.IsDeleted == false)
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                users = users
                    .Where(u =>
                        u.FullName.ToLower().Contains(keyword) ||
                        u.Email.ToLower().Contains(keyword))
                    .ToList();

                ViewBag.Keyword = keyword;
            }

            return View(users);
        }

        // GET: /User/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            User user = db.Users.FirstOrDefault(u =>
                u.Id == id &&
                u.IsActive == true);

            if (user == null)
            {
                return HttpNotFound();
            }

            var topics = db.Topics
                .Include("User")
                .Include("TopicTags.Tag")
                .Where(t =>
                    t.UserId == id &&
                    t.IsDeleted == false)
                .OrderByDescending(t => t.CreatedAt)
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

            UserDetailsVM model = new UserDetailsVM
            {
                User = user,

                TotalTopics = db.Topics.Count(t =>
                    t.UserId == id &&
                    t.IsDeleted == false),

                TotalVotes = db.Topics
                    .Where(t =>
                        t.UserId == id &&
                        t.IsDeleted == false)
                    .Select(t => t.VoteCount)
                    .DefaultIfEmpty(0)
                    .Sum(),

                TotalComments = db.TopicComments.Count(c =>
                    c.UserId == id &&
                    c.IsDeleted == false),

                Topics = topics
            };

            return View(model);
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