using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class AutoTestcaseRequest
    {
        public int ProblemId { get; set; }
        public string GeneratorSource { get; set; }
        public string GeneratorLanguage { get; set; }
        public string SolutionSource { get; set; }
        public string SolutionLanguage { get; set; }
        public int NumberOfTestcases { get; set; }
    }

}
