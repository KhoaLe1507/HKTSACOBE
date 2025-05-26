using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class BlogReaction
    {
        [Required] public int BlogReactionId { get; set; }
        [Required] public int PostId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string ReactionType { get; set; }
        [Required] public DateTime ReactedAt { get; set; }
    }
}
