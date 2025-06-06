using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class EditProfileRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Bio { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string? School { get; set; }
    }

}
