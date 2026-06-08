using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace TechForum.Models
{
    public class TopicCreateVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }

        public List<int> SelectedTagIds { get; set; }

        public IEnumerable<Tag> AvailableTags { get; set; }

        public IEnumerable<HttpPostedFileBase> ImageFiles { get; set; }
    }
}