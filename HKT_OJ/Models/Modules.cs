using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.Models
{
    public class Modules
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public int SectionId { get; set; }
        [Required] public int Order { get; set; }
    }

}
