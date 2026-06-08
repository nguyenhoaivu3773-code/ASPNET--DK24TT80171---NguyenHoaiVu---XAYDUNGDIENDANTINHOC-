using System;

namespace TechForum.Models
{
    public class AdminTopicItemVM
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string ShortContent { get; set; }

        public string UserFullName { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ViewCount { get; set; }

        public int VoteCount { get; set; }

        public int CommentCount { get; set; }

        public bool IsDeleted { get; set; }
    }
}