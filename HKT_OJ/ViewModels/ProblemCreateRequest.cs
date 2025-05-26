using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class ProblemCreateRequest
    {
        public string Name { get; set; }
        public string Frequent { get; set; }
        public int ModuleContentId { get; set; }
        public string Difficulty { get; set; }
        public int TimeLimit { get; set; } // seconds
        public int MemoryLimit { get; set; } // KB
        public string ProblemStatement { get; set; }
        public string FormatInput { get; set; }
        public string FormatOutput { get; set; }
        public int NumberOfGeneratedTestcases { get; set; }

        public string TestGeneratorSource { get; set; }  // Code sinh test
        public string TestGeneratorLanguage { get; set; } // Language của code sinh test

        public List<ConstraintRequest> Constraints { get; set; }
        public List<TestcaseRequest> SampleTestcases { get; set; }
        public SolutionRequest Solution { get; set; }
    }

    public class ConstraintRequest
    {
        public string Variable { get; set; }
        public long MinValue { get; set; }
        public long MaxValue { get; set; }
    }

    public class TestcaseRequest
    {
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public string Explanation { get; set; }
    }

    public class SolutionRequest
    {
        public string Source { get; set; }
        public string Language { get; set; }
        public string Explanation { get; set; }
    }

}
