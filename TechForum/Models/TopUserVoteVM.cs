namespace TechForum.Models
{
    public class TopUserVoteVM
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Avatar { get; set; }

        public int TotalVotes { get; set; }

        public int TotalTopics { get; set; }
    }
}