using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class User
    {
        [Required] public int UserId { get; set; }
        [Required] public string FullName { get; set; }
        [Required] public string UserName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string PasswordHash { get; set; }
        [Required] public int Role { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required] public string Gender { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public bool IsActive { get; set; }
        [Required] public DateTime CreatedAt { get; set; }
        [Required] public string School { get; set; }
        [Required] public int ProblemSolved { get; set; } = 0;
        [Required] public int ProblemCreated { get; set; } = 0;
    }

}
