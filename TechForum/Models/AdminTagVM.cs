using System;
using System.Collections.Generic;

namespace TechForum.Models
{
    public class AdminTagVM
    {
        public List<AdminTagItemVM> Tags { get; set; }

        public string NewTagName { get; set; }
    }

    public class AdminTagItemVM
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public int TopicCount { get; set; }
    }
}