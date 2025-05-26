    using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class ProblemCardViewModel
    {
        public int ProblemId { get; set; }
        public string Name { get; set; }
        public string ModuleContentTitle { get; set; }
        public string Difficulty { get; set; }
        public string Frequent { get; set; }
        public string Section { get; set; }
        public string Status { get; set; } // Completed, In Progress, Not Started

        public int AuthorId { get; set; }
    }

}
