using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class Problem
    {
        [Required] public int ProblemId { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string ProblemStatement { get; set; }
        [Required] public int TimeLimit { get; set; }
        [Required] public int MemoryLimit { get; set; }
        [Required] public string FormatInput { get; set; }
        [Required] public string FormatOutput { get; set; }
        [Required] public int SolutionId { get; set; }
        [Required] public string Difficulty { get; set; }
        [Required] public int ModuleContentId { get; set; }
        [Required] public string Frequent { get; set; }
        [Required] public int AuthorId { get; set; }
    }
}
