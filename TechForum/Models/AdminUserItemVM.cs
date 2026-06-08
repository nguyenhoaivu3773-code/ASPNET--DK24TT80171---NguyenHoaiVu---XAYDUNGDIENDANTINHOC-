namespace TechForum.Models
{
    public class AdminUserItemVM
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public int TotalTopics { get; set; }

        public bool IsActive { get; set; }
    }
}