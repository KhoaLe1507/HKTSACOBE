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
        public async Task<IActionResult> RegisterAsStudent([FromForm] RegisterRequest request, IFormFile? AvatarFile)
        {
            request.Role = 0;

            if (_context.User.Any(u => u.UserName == request.UserName || u.Email == request.Email))
                return BadRequest("Username or Email already exists");

            string? avatarPath = null;
            if (AvatarFile != null)
            {
                var folderPath = Path.Combine("wwwroot", "avatars");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(AvatarFile.FileName);
                var savePath = Path.Combine(folderPath, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await AvatarFile.CopyToAsync(stream);
                avatarPath = "/avatars/" + fileName;
            }

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
                Bio = request.Bio,
                AvatarUrl = avatarPath,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ProblemCreated = 0,
                ProblemSolved = 0
            };

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Student registered successfully");
        }

        [HttpPost("edit-profile/{id}")]
        [Authorize]
        public async Task<IActionResult> EditProfile(int id, [FromForm] EditProfileRequest request, IFormFile? AvatarFile)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound("User not found");

            // xử lý ảnh nếu có
            if (AvatarFile != null)
            {
                var folderPath = Path.Combine("wwwroot", "avatars");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(AvatarFile.FileName);
                var savePath = Path.Combine(folderPath, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await AvatarFile.CopyToAsync(stream);
                user.AvatarUrl = "/avatars/" + fileName;
            }

            // Chỉ cập nhật các trường được cho phép
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Bio = request.Bio;
            user.BirthDate = request.BirthDate;
            user.Gender = request.Gender;
            user.PhoneNumber = request.PhoneNumber;
            user.School = request.School;

            _context.User.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully");
        }


        [HttpPost("register-by-admin")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RegisterByAdmin([FromForm] RegisterRequest request, IFormFile? AvatarFile)
        {
            if (_context.User.Any(u => u.UserName == request.UserName || u.Email == request.Email))
                return BadRequest("Username or Email already exists");

            string? avatarPath = null;
            if (AvatarFile != null)
            {
                var folderPath = Path.Combine("wwwroot", "avatars");

                // Nếu thư mục chưa tồn tại thì tạo mới
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var fileName = Guid.NewGuid() + Path.GetExtension(AvatarFile.FileName);
                var savePath = Path.Combine("wwwroot/avatars", fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await AvatarFile.CopyToAsync(stream);
                avatarPath = "/avatars/" + fileName;
            }

            var newUser = new User
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber,
                BirthDate = request.BirthDate,
                School = request.School,
                AvatarUrl = avatarPath,
                Bio = request.Bio,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ProblemCreated = 0,
                ProblemSolved = 0
            };

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

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

        [HttpGet("profile/{id}")]
        [Authorize]
        public IActionResult GetUserProfile(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = int.Parse(User.FindFirstValue(ClaimTypes.Role)); // người xem

            var targetUser = _context.User.FirstOrDefault(u => u.UserId == id);
            if (targetUser == null) return NotFound("User not found");

            var result = new
            {
                targetUser.FullName,
                targetUser.UserName,
                targetUser.AvatarUrl,
                targetUser.Bio,
                targetUser.BirthDate,
                targetUser.Gender,
                targetUser.Email,
                targetUser.PhoneNumber,

                // Chỉ admin được xem các trường bên dưới
                Role = targetUser.Role,
                IsActive = currentUserRole == 2 ? targetUser.IsActive : (bool?)null,
                CreatedAt = currentUserRole == 2 ? targetUser.CreatedAt : (DateTime?)null,

                // Chỉ hiển thị nếu profile của Student hoặc Professor
                School = (targetUser.Role == 0 || targetUser.Role == 1) ? targetUser.School : null,

                // Chỉ hiển thị nếu target là Student
                ProblemSolved = targetUser.Role == 0 ? targetUser.ProblemSolved : (int?)null,

                // Chỉ hiển thị nếu target là Professor
                ProblemCreated = targetUser.Role == 1 ? targetUser.ProblemCreated : (int?)null
            };

            return Ok(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "2")] // Admin only
        public IActionResult GetAllUsers()
        {
            var users = _context.User.Select(u => new {
                u.UserId,
                u.UserName,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Role
            }).ToList();

            return Ok(users);
        }


        [HttpGet("ranking/students")]
        public IActionResult GetStudentRanking()
        {
            var result = _context.User
                .Where(u => u.Role == 0)
                .OrderByDescending(u => u.ProblemSolved)
                .Select(u => new {
                    u.UserId,
                    u.UserName,
                    u.FullName,
                    u.School,
                    u.ProblemSolved
                }).ToList();

            return Ok(result);
        }

        [HttpGet("ranking/professors")]
        public IActionResult GetProfessorRanking()
        {
            var result = _context.User
                .Where(u => u.Role == 1)
                .OrderByDescending(u => u.ProblemCreated)
                .Select(u => new {
                    u.UserId,
                    u.UserName,
                    u.FullName,
                    u.School,
                    u.ProblemCreated
                }).ToList();

            return Ok(result);
        }



        [HttpDelete("{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound();

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully");
        }




    }

}
