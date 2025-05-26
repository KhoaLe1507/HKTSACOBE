using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class Testcase
    {
        [Required] public int TestcaseId { get; set; }
        [Required] public int ProblemId { get; set; }
        [Required] public string InputPath { get; set; }
        [Required] public string OutputPath { get; set; }
        [Required] public bool IsSample { get; set; }
        public string? Explanation { get; set; }
    }
}
