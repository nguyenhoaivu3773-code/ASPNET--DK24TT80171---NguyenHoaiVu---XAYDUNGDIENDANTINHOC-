using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("TopicComments")]
    public class TopicComment
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public int? ParentCommentId { get; set; }

        public int Level { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual User User { get; set; }

        public virtual TopicComment ParentComment { get; set; }

        public virtual ICollection<TopicComment> Replies { get; set; }
    }
}