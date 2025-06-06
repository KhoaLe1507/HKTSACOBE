using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class ProblemUpdateRequest
    {
        public string Name { get; set; }
        public string ProblemStatement { get; set; }
        public int TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
        public string FormatInput { get; set; }
        public string FormatOutput { get; set; }
        public string Difficulty { get; set; }
        public int ModuleContentId { get; set; }
        public string Frequent { get; set; }

        public string SolutionLanguage { get; set; }
        public string SolutionExplanation { get; set; }
        public string SolutionSource { get; set; }

        public List<ConstraintDto> Constraints { get; set; }
    }

    public class ConstraintDto
    {
        public string Variable { get; set; }
        public long MinValue { get; set; }
        public long MaxValue { get; set; }
    }

}
