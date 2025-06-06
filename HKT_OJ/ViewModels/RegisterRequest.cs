using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Role { get; set; } // 0: Student, 1: Professor, 2: Admin
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string? School { get; set; }

        public string? AvatarUrl { get; set; } // mới thêm
        public string? Bio { get; set; }       // mới thêm
    }
}
