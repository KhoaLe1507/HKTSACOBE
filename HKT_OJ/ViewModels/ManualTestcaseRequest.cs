using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class ManualTestcaseRequest
    {
        public int ProblemId { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Explanation { get; set; }
    }

}
