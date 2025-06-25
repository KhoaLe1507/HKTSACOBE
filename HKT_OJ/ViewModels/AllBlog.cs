using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class AllBlog : Controller
    {
        public int PostId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Visibility { get; set; } = "Private";
        public int UserId { get; set; }

        public string ApprovalStatus { get; set; }
    }
}
