using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class BlogComment
    {
        [Required] public int BlogCommentId { get; set; }
        [Required] public int PostId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string Content { get; set; }
        [Required] public DateTime CreatedAt { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
