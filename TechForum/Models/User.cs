using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForum.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        [StringLength(255)]
        public string Avatar { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        [StringLength(255)]
        public string RememberTokenHash { get; set; }

        public DateTime? RememberTokenExpiry { get; set; }
    }
}