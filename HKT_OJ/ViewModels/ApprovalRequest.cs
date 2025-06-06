using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class ApprovalRequest
    {
        [Required]
        public string NewStatus { get; set; } = "Approved"; // "Approved" hoặc "Rejected"
    }
}
