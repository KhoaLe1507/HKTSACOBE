using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class BlogPostRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Visibility { get; set; } = "Public";
    }
}
