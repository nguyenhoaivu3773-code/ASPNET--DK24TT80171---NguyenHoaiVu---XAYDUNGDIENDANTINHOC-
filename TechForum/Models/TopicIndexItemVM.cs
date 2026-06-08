using System;
using System.Collections.Generic;

namespace TechForum.Models
{
    public class TopicIndexItemVM
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int? UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserAvatar { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ViewCount { get; set; }

        public int VoteCount { get; set; }

        public int CommentCount { get; set; }

        public List<Tag> Tags { get; set; }
    }
}