using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class GenerateInputCodeRequest
    {
        public string InputFormat { get; set; }
        public string Language { get; set; }
        public string? Description { get; set; }
        public List<ConstraintDto> Constraints { get; set; } = new();
        public string? SampleInput { get; set; }
    }


}
