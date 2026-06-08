using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("TopicVotes")]
    public class TopicVote
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual User User { get; set; }
    }
}