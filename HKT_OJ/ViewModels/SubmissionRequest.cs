namespace HKT_OJ.Models
{
    public class SubmissionRequest
    {
        public string SourceCode { get; set; }
        public string LanguageId { get; set; }
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public float? TimeLimit { get; set; } = 2.0f;
        public int? MemoryLimit { get; set; } = 262144;
    }
}
