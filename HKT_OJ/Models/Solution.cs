using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class Solution
    {
        [Required] public int SolutionId { get; set; }

        [Required] public int ProblemId { get; set; }

        [Required] public string Explanation { get; set; }

        [Required] public string Language { get; set; }

        [Required] public string Source { get; set; }
    }
}
