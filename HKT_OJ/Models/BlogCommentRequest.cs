using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class BlogCommentRequest
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
