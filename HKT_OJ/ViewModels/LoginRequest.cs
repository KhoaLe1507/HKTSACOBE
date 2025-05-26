using Microsoft.AspNetCore.Mvc;

namespace HKT_OJ.ViewModels
{
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
