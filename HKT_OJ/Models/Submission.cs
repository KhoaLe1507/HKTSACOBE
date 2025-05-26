using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class Submission
    {
        [Required] public int SubmissionId { get; set; }
        [Required] public int ProblemId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public int Time { get; set; }
        [Required] public int Memory { get; set; }
        [Required] public string Language { get; set; }
        [Required] public DateTime SubmitAtTime { get; set; }
        [Required] public string Result { get; set; }

        [Required] public string UserSourceCode { get; set; }
    }
}
