using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class SampleTestcaseUpdateRequest
    {
        public List<SampleTestDto> ToAdd { get; set; }
        public List<SampleTestDto> ToUpdate { get; set; }
        public List<int> ToDelete { get; set; }
    }

    public class SampleTestDto
    {
        public int? TestcaseId { get; set; } // optional for Add
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public string Explanation { get; set; }
    }

}
