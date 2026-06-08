using System.Collections.Generic;

namespace TechForum.Models
{
    public class TopicDetailsVM
    {
        public Topic Topic { get; set; }

        public List<TopicComment> ParentComments { get; set; }

        public bool HasVoted { get; set; }

        public List<TopicIndexItemVM> AuthorTopVotedTopics { get; set; }
    }
}