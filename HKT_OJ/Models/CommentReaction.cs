using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class CommentReaction
    {
        [Required] public int CommentReactionId { get; set; }
        [Required] public int CommentId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string ReactionType { get; set; }
        [Required] public DateTime ReactedAt { get; set; }
    }
}
