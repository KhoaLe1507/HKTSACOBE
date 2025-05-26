using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class AddModuleRequest
    {
        public string Name { get; set; }
        public int SectionId { get; set; }
        public string Position { get; set; }
        public int ReferenceId { get; set; }
    }
}
