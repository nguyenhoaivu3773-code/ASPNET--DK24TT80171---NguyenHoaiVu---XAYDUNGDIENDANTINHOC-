using System.Data.Entity;

namespace TechForum.Models
{
    public class TechForumDbContext : DbContext
    {
        public TechForumDbContext() : base("name=TechForumDbContext")
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Topic> Topics { get; set; }

        public DbSet<TopicImage> TopicImages { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<TopicTag> TopicTags { get; set; }

        public DbSet<TopicComment> TopicComments { get; set; }

        public DbSet<TopicVote> TopicVotes { get; set; }

        public DbSet<TopicReport> TopicReports { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TopicTag>()
                .HasKey(x => new { x.TopicId, x.TagId });

            modelBuilder.Entity<TopicTag>()
                .HasRequired(x => x.Topic)
                .WithMany(x => x.TopicTags)
                .HasForeignKey(x => x.TopicId);

            modelBuilder.Entity<TopicTag>()
                .HasRequired(x => x.Tag)
                .WithMany(x => x.TopicTags)
                .HasForeignKey(x => x.TagId);

            modelBuilder.Entity<TopicComment>()
                .HasOptional(x => x.ParentComment)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentCommentId);

            modelBuilder.Entity<TopicVote>()
                .HasRequired(x => x.Topic)
                .WithMany(x => x.TopicVotes)
                .HasForeignKey(x => x.TopicId);

            modelBuilder.Entity<TopicVote>()
                .HasRequired(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<TopicReport>()
                .HasRequired(x => x.Topic)
                .WithMany()
                .HasForeignKey(x => x.TopicId);

            modelBuilder.Entity<TopicReport>()
                .HasRequired(x => x.ReporterUser)
                .WithMany()
                .HasForeignKey(x => x.ReporterUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TopicReport>()
                .HasOptional(x => x.HandledByUser)
                .WithMany()
                .HasForeignKey(x => x.HandledByUserId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}