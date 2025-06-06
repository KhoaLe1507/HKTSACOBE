using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class NonSampleTestcaseUpdateRequest
    {
        public List<NonSampleTestDto> ToAdd { get; set; }
        public List<NonSampleTestDto> ToUpdate { get; set; }
        public List<int> ToDelete { get; set; }
    }

    public class NonSampleTestDto
    {
        public int? TestcaseId { get; set; } // null với test mới
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public string Explanation { get; set; }
    }

}
