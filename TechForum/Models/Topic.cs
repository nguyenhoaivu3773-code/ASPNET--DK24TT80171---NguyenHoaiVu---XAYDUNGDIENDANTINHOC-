using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("Topics")]
    public class Topic
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int UserId { get; set; }

        public int ViewCount { get; set; }

        public int VoteCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<TopicImage> TopicImages { get; set; }

        public virtual ICollection<TopicTag> TopicTags { get; set; }

        public virtual ICollection<TopicComment> TopicComments { get; set; }

        public virtual ICollection<TopicVote> TopicVotes { get; set; }
    }
}