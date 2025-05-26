using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class ProblemProgress
    {
        [Required] public int Id { get; set; }

        [Required]  public int UserId { get; set; }
        [Required]  public int ProblemId { get; set; }

        [Required]  public string Status { get; set; } = "Not Started"; // "In Progress", "Completed"

    }

}
