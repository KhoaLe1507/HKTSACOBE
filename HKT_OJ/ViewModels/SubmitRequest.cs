using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class SubmitRequest
    {
        public string SourceCode { get; set; }
        public string LanguageId { get; set; }
    }
}
