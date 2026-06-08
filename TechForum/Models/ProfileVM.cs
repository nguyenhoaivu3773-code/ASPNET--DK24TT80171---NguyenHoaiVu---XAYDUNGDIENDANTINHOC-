using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace TechForum.Models
{
    public class ProfileVM
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Avatar { get; set; }

        public HttpPostedFileBase AvatarFile { get; set; }

        public int TotalTopics { get; set; }

        public int TotalVotes { get; set; }

        public int TotalComments { get; set; }

        public string Filter { get; set; }

        public List<TopicIndexItemVM> Topics { get; set; }
    }
}