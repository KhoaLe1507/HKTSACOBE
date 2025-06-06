using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class GenerateTestcaseRequest
    {
        public int NumberOfTestcases { get; set; }

        public string TestGeneratorSource { get; set; }
        public string TestGeneratorLanguage { get; set; }

        public string SolutionSource { get; set; }
        public string SolutionLanguage { get; set; }
    }
}
