using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("TopicReports")]
    public class TopicReport
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public int ReporterUserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        [StringLength(500)]
        public string AdminNote { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? HandledAt { get; set; }

        public int? HandledByUserId { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual User ReporterUser { get; set; }

        public virtual User HandledByUser { get; set; }
    }
}