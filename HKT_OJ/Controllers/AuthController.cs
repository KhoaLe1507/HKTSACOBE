using HKT_OJ.Data;
using HKT_OJ.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using HKT_OJ.Models;
using Microsoft.AspNetCore.Authorization;

namespace HKT_OJ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        [HttpPost("register")]
        public IActionResult RegisterAsStudent([FromBody] RegisterRequest request)
        {
            // Role luôn là Student
            request.Role = 0;

            if (_context.User.Any(u => u.UserName == request.UserName || u.Email == request.Email))
                return BadRequest("Username or Email already exists");

            var newUser = new User
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),


                Role = 0,


                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber,
                BirthDate = request.BirthDate,
                School = request.School,
                AvatarUrl = request.AvatarUrl,
                Bio = request.Bio,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ProblemCreated = 0,
                ProblemSolved = 0
            };

            _context.User.Add(newUser);
            _context.SaveChanges();

            return Ok("Student registered successfully");
        }

        [Authorize(Roles = "2")] //Chỉ admin truy cập được api này
        [HttpPost("register-by-admin")]
        public IActionResult RegisterByAdmin([FromBody] RegisterRequest request)
        {
            if (_context.User.Any(u => u.UserName == request.UserName || u.Email == request.Email))
                return BadRequest("Username or Email already exists");

            var newUser = new User
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),


                Role = request.Role, /// KHAC NHAU


                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber,
                BirthDate = request.BirthDate,
                School = request.School,
                AvatarUrl = request.AvatarUrl,
                Bio = request.Bio,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ProblemCreated = 0,
                ProblemSolved = 0
            };

            _context.User.Add(newUser);
            _context.SaveChanges();

            return Ok("User created by Admin successfully");
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.User.FirstOrDefault(u => u.UserName == request.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:ExpiryMinutes"])),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);

            return Ok(new LoginResponse
            {
                AccessToken = jwt,
                Role = user.Role.ToString(),
                UserId = user.UserId
            });
        }
    }

}
