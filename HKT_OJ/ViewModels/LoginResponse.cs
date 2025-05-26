using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
    }
}
