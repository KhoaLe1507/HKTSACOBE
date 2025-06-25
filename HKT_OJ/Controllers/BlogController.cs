using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HKT_OJ.Data;
using HKT_OJ.Models;
using HKT_OJ.Services;
using HKT_OJ.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HKT_OJ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ API Add Blog
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddBlog([FromBody] BlogPostRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            // 🔍 Lấy Role của user
            var role = await _context.User
                .Where(u => u.UserId == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            var blog = new BlogPost
            {
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                Visibility = request.Visibility,
                ApprovalStatus = role == 2 ? "Approved" : "Pending" // ✅ Nếu là Admin thì duyệt luôn
            };

            _context.BlogPost.Add(blog);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                postId = blog.BlogPostId,
                message = "Blog post created successfully"
            });
        }





        // ✅ API Edit Blog
        [Authorize]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditBlog(int id, [FromBody] BlogPostRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var blog = await _context.BlogPost.FirstOrDefaultAsync(b => b.BlogPostId == id && b.UserId == int.Parse(userId));
            if (blog == null) return NotFound("Blog not found or permission denied.");

            blog.Title = request.Title;
            blog.Content = request.Content;
            blog.ImageUrl = request.ImageUrl;
            blog.Visibility = request.Visibility;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Blog post updated successfully" });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            var blog = await _context.BlogPost.FirstOrDefaultAsync(b => b.BlogPostId == id);
            if (blog == null) return NotFound("Blog not found");

            // Chỉ cho phép người dùng chính sửa bài của mình
            if (blog.UserId != currentUserId)
                return Forbid("You do not have permission to view this blog.");

            return Ok(new
            {
                title = blog.Title,
                content = blog.Content,
                visibility = blog.Visibility,
                imageUrl = blog.ImageUrl
            });
        }

        [HttpGet("list-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _context.BlogPost
                .Where(b => b.ApprovalStatus == "Approved" && b.Visibility == "Public") // ✅ Lọc thêm Visibility
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new AllBlog
                {
                    PostId = b.BlogPostId,
                    Title = b.Title,
                    Content = b.Content,
                    ImageUrl = b.ImageUrl,
                    CreatedAt = b.CreatedAt,
                    Visibility = b.Visibility,
                    UserId = b.UserId, // ✅ THÊM DÒNG NÀY

                    AuthorName = _context.User
                        .Where(u => u.UserId == b.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "Unknown",

                    Role = _context.User
                        .Where(u => u.UserId == b.UserId)
                        .Select(u => u.Role == 0 ? "Student" : u.Role == 1 ? "Professor" : "Admin")
                        .FirstOrDefault() ?? "Unknown"
                })

                .ToListAsync();

            foreach (var b in blogs)
            {
                Console.WriteLine($"📌 Blog: {b.Title} - UserId = {b.UserId}");
            }

            return Ok(blogs);
        }

        [Authorize]
        [HttpDelete("delete/{postId}")]
        public async Task<IActionResult> DeleteBlog(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null)
                return Unauthorized("User not logged in.");

            int userId = int.Parse(userIdClaim);
            int role = int.Parse(roleClaim);

            var blog = await _context.BlogPost.FindAsync(postId);
            if (blog == null)
                return NotFound("Blog post not found.");

            // ✅ Chỉ cho phép:
            // - Admin (role == 2)
            // - Tác giả (blog.UserId == userId)
            if (role != 2 && blog.UserId != userId)
                return Forbid("Bạn không có quyền xoá bài viết này.");

            // ✅ Xoá tất cả comment thuộc bài viết
            var relatedComments = _context.BlogComment
                .Where(c => c.PostId == postId);
            _context.BlogComment.RemoveRange(relatedComments);

            _context.BlogPost.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Blog and related comments deleted." });
        }


        [Authorize]
        [HttpGet("my-blogs")]
        public async Task<IActionResult> GetMyBlogs()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("User not logged in.");

            int userId = int.Parse(userIdClaim.Value);

            var blogs = await _context.BlogPost
                .Where(b => b.UserId == userId) // ✅ Không lọc trạng thái
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new AllBlog
                {
                    AuthorName = _context.User
                        .Where(u => u.UserId == b.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "Unknown",

                    Role = _context.User
                        .Where(u => u.UserId == b.UserId)
                        .Select(u => u.Role == 0 ? "Student" : u.Role == 1 ? "Professor" : "Admin")
                        .FirstOrDefault() ?? "Unknown",

                    PostId = b.BlogPostId,
                    Title = b.Title,
                    Content = b.Content,
                    ImageUrl = b.ImageUrl,
                    CreatedAt = b.CreatedAt,
                    Visibility = b.Visibility,

                    ApprovalStatus = b.ApprovalStatus // ✅ Trả về trạng thái để FE hiển thị
                })
                .ToListAsync();

            return Ok(blogs);
        }


        [Authorize(Roles = "2")] // ✅ Chỉ cho Admin
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingBlogs()
        {
            var pendingBlogs = await _context.BlogPost
                .Where(b => b.ApprovalStatus == "Pending")
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new AllBlog
                {
                    AuthorName = _context.User
                        .Where(u => u.UserId == b.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "Unknown",

                    Role = _context.User
                        .Where(u => u.UserId == b.UserId)
                        .Select(u => u.Role == 0 ? "Student" : u.Role == 1 ? "Professor" : "Admin")
                        .FirstOrDefault() ?? "Unknown",

                    PostId = b.BlogPostId,
                    Title = b.Title,
                    Content = b.Content,
                    ImageUrl = b.ImageUrl,
                    CreatedAt = b.CreatedAt,
                    Visibility = b.Visibility,
                    ApprovalStatus = b.ApprovalStatus // ✅ Trả về để Admin thấy
                })
                .ToListAsync();

            return Ok(pendingBlogs);
        }

        [HttpPost("approve/{postId}")]
        public async Task<IActionResult> ApproveOrRejectBlog(int postId, [FromBody] ApprovalRequest request)
        {
            var blog = await _context.BlogPost.FirstOrDefaultAsync(b => b.BlogPostId == postId);
            if (blog == null)
                return NotFound(new { message = "Blog post not found." });

            if (request.NewStatus != "Approved" && request.NewStatus != "Rejected")
                return BadRequest(new { message = "Invalid status. Use 'Approved' or 'Rejected'." });

            blog.ApprovalStatus = request.NewStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Blog post has been {request.NewStatus.ToLower()}." });
        }


        [Authorize]
        [HttpPost("comment")]
        public async Task<IActionResult> AddComment([FromBody] BlogCommentRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var comment = new BlogComment
            {
                PostId = request.PostId,
                UserId = int.Parse(userIdClaim.Value),
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = request.ParentCommentId
            };

            _context.BlogComment.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment added successfully." });
        }

        [HttpGet("comments/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.BlogComment
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new {
                    c.BlogCommentId,
                    c.Content,
                    c.UserId,
                    c.CreatedAt,
                    c.ParentCommentId,
                    User = _context.User
                        .Where(u => u.UserId == c.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "Unknown",
                    Role = _context.User
                        .Where(u => u.UserId == c.UserId)
                        .Select(u => u.Role == 0 ? "Student" : u.Role == 1 ? "Professor" : "Admin")
                        .FirstOrDefault() 
                })
                .ToListAsync();

            return Ok(comments);
        }

        [Authorize]
        [HttpDelete("comment/delete/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.BlogComment.FindAsync(commentId);
            if (comment == null)
                return NotFound();

            var replies = _context.BlogComment
                .Where(c => c.ParentCommentId == commentId);
            _context.BlogComment.RemoveRange(replies);

            _context.BlogComment.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }



    }

}
