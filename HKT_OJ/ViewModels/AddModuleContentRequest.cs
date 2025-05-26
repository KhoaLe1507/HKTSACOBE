using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class AddModuleContentRequest
    {
        public string Title { get; set; }
        public string Frequent { get; set; }
        public string Description { get; set; }
        public string HtmlContentPath { get; set; }
        public int AuthorId { get; set; }
        public int ModuleId { get; set; }
        public string Position { get; set; }
        public int ReferenceId { get; set; }
    }
}
