using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class BlogPost
    {
        [Required] public int BlogPostId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Content { get; set; }
        public string? ImageUrl { get; set; }
        [Required] public DateTime CreatedAt { get; set; }
        [Required] public string Visibility { get; set; }

        [Required] public string ApprovalStatus { get; set; } = "Pending";
    }
}
