namespace TechForum.Models
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }

        public int TotalTopics { get; set; }

        public int TotalTags { get; set; }

        public int PendingReports { get; set; }
    }
}