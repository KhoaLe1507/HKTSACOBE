using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class BlogAnalysisRequest
    {
        public string Content { get; set; }
        public string? Criteria { get; set; }
    }

}
