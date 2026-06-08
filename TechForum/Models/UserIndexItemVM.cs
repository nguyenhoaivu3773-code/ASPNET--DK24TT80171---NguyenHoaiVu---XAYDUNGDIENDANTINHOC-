namespace TechForum.Models
{
    public class UserIndexItemVM
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Avatar { get; set; }

        public int TotalTopics { get; set; }

        public int TotalVotes { get; set; }

        public int TotalComments { get; set; }
    }
}