using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class SubmissionResult
    {
        [Required] public int SubmissionResultId { get; set; }
        [Required] public int SubmissionId { get; set; }
        [Required] public int TestcaseId { get; set; }
        [Required] public string InputPath { get; set; }
        [Required] public string ExpectedOutputPath { get; set; }
        [Required] public string UserOutputPath { get; set; }
        [Required] public int ExecutedTime { get; set; }
        [Required] public int Memory { get; set; }
        [Required] public string Result { get; set; }
    }
}
