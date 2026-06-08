using System.Collections.Generic;

namespace TechForum.Models
{
    public class UserDetailsVM
    {
        public User User { get; set; }

        public int TotalTopics { get; set; }

        public int TotalVotes { get; set; }

        public int TotalComments { get; set; }

        public List<TopicIndexItemVM> Topics { get; set; }
    }
}