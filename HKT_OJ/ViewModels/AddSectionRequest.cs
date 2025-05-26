using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class AddSectionRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Position { get; set; } // "At the end", "Behind", "Front"
        public int ReferenceId { get; set; }
    }

}
