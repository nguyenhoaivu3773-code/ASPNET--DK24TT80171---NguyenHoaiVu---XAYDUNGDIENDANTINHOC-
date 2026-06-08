using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("TopicImages")]
    public class TopicImage
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        [Required]
        [StringLength(255)]
        public string ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Topic Topic { get; set; }
    }
}