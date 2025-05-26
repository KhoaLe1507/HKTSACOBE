using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class ModuleContent
    {
        [Required] public int Id { get; set; }
        [Required] public int ModuleId { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Frequent { get; set; }
        [Required] public string Description { get; set; }
        [Required] public string HtmlContentPath { get; set; }
        [Required] public int Order { get; set; }
        [Required] public int AuthorId { get; set; }
        [Required] public DateTime CreatedAt { get; set; }
    }
}
