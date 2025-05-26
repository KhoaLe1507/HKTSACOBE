using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class ProblemConstraint
    {
        [Required] public int ProblemConstraintId { get; set; }
        [Required] public int ProblemId { get; set; }
        [Required] public string Variable { get; set; }
        [Required] public long MinValue { get; set; }
        [Required] public long MaxValue { get; set; }
    }
}
