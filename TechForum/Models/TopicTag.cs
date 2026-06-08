using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("TopicTags")]
    public class TopicTag
    {
        public int TopicId { get; set; }

        public int TagId { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual Tag Tag { get; set; }
    }
}