using System.Collections.Generic;

namespace TechForum.Models
{
    public class TopicIndexVM
    {
        public List<TopicIndexItemVM> Topics { get; set; }

        public List<Tag> Tags { get; set; }

        public int? SelectedTagId { get; set; }

        public string Keyword { get; set; }

        public List<TopUserVoteVM> TopUsersByVotes { get; set; }

        public string Sort { get; set; }
    }
}