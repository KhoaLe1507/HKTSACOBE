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
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "1")]
        [HttpGet("professor-dashboard")]
        public IActionResult GetProfessorDashboardStats(DateTime startDate, DateTime endDate)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not logged in.");

            int professorId = int.Parse(userIdClaim);

            // ModuleContent
            var myContents = _context.ModuleContent
                .Where(mc => mc.AuthorId == professorId &&
                             mc.CreatedAt >= startDate && mc.CreatedAt <= endDate)
                .ToList();
            var totalContents = myContents.Count;

            var contentBySection = myContents
                .Join(_context.Modules, mc => mc.ModuleId, m => m.Id, (mc, m) => new { mc, m })
                .Join(_context.Sections, mm => mm.m.SectionId, s => s.Id, (mm, s) => s.Name)
                .GroupBy(name => name)
                .Select(g => new { Section = g.Key, Count = g.Count() })
                .ToList();

            var contentByFrequent = myContents
                .GroupBy(mc => mc.Frequent)
                .Select(g => new { Frequent = g.Key, Count = g.Count() })
                .ToList();

            // Problem
            var myProblems = _context.Problem
                .Where(p => p.AuthorId == professorId)
                .ToList();

            var totalProblems = myProblems.Count;
            var problemsByDifficulty = myProblems
                .GroupBy(p => p.Difficulty)
                .Select(g => new { Difficulty = g.Key, Count = g.Count() })
                .ToList();

            // Blog
            var myBlogs = _context.BlogPost
                .Where(b => b.UserId == professorId &&
                            b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .ToList();

            var totalBlogs = myBlogs.Count;
            var blogsByStatus = myBlogs
                .GroupBy(b => b.ApprovalStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return Ok(new
            {
                ModuleContent = new
                {
                    Total = totalContents,
                    BySection = contentBySection,
                    ByFrequent = contentByFrequent
                },
                Problem = new
                {
                    Total = totalProblems,
                    ByDifficulty = problemsByDifficulty
                },
                Blog = new
                {
                    Total = totalBlogs,
                    ByStatus = blogsByStatus
                }
            });
        }

    }
}
