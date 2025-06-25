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

            // MODULE CONTENT
            var myContents = _context.ModuleContent
                .Where(mc => mc.AuthorId == professorId &&
                             mc.CreatedAt >= startDate && mc.CreatedAt <= endDate)
                .ToList();

            var contentBySection = myContents
                .Join(_context.Modules, mc => mc.ModuleId, m => m.Id, (mc, m) => new { mc, m })
                .Join(_context.Sections, mm => mm.m.SectionId, s => s.Id, (mm, s) => s.Name)
                .GroupBy(name => name)
                .Select(g => new { section = g.Key, count = g.Count() })
                .ToList();

            var contentByFrequent = myContents
                .GroupBy(mc => mc.Frequent)
                .Select(g => new { frequent = g.Key, count = g.Count() })
                .ToList();

            var recentModuleContents = myContents
                .Join(_context.Modules, mc => mc.ModuleId, m => m.Id, (mc, m) => new {
                    mc.Id,
                    mc.Title,
                    mc.CreatedAt,
                    ModuleName = m.Name,
                    SectionName = _context.Sections.FirstOrDefault(s => s.Id == m.SectionId)!.Name
                })
                .OrderByDescending(mc => mc.CreatedAt)
                .Take(3)
                .ToList();

            // PROBLEMS
            var myProblems = _context.Problem
                .Where(p => p.AuthorId == professorId)
                .ToList();

            var problemsByDifficulty = myProblems
                .GroupBy(p => p.Difficulty)
                .Select(g => new { difficulty = g.Key, count = g.Count() })
                .ToList();

            var topProblemsBySubmission = _context.Problem
                .Where(p => p.AuthorId == professorId)
                .Join(_context.ModuleContent, p => p.ModuleContentId, mc => mc.Id, (p, mc) => new { p, mc })
                .Join(_context.Modules, pmc => pmc.mc.ModuleId, m => m.Id, (pmc, m) => new { pmc.p, pmc.mc, m })
                .Join(_context.Sections, pmcm => pmcm.m.SectionId, s => s.Id, (pmcm, s) => new {
                    pmcm.p.ProblemId,
                    ProblemName = pmcm.p.Name,
                    pmcm.p.Difficulty,
                    ModuleContentTitle = pmcm.mc.Title,
                    ModuleName = pmcm.m.Name,
                    SectionName = s.Name,
                    SubmissionCount = _context.Submission.Count(sub => sub.ProblemId == pmcm.p.ProblemId)
                })
                .OrderByDescending(x => x.SubmissionCount)
                .Take(3)
                .ToList();

            // BLOGS
            var myBlogs = _context.BlogPost
                .Where(b => b.UserId == professorId &&
                            b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .ToList();

            var blogsByStatus = myBlogs
                .GroupBy(b => b.ApprovalStatus)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToList();

            var topBlogsByComment = _context.BlogPost
                .Where(b => b.UserId == professorId &&
                            b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .Select(b => new {
                    b.BlogPostId,
                    b.Title,
                    b.Visibility,
                    b.CreatedAt,
                    CommentCount = _context.BlogComment.Count(c => c.PostId == b.BlogPostId)
                })
                .OrderByDescending(x => x.CommentCount)
                .Take(3)
                .ToList();

            return Ok(new
            {
                moduleContent = new
                {
                    total = myContents.Count,
                    bySection = contentBySection,
                    byFrequent = contentByFrequent,
                    recentModuleContents
                },
                problem = new
                {
                    total = myProblems.Count,
                    byDifficulty = problemsByDifficulty,
                    topProblemsBySubmission
                },
                blog = new
                {
                    total = myBlogs.Count,
                    byStatus = blogsByStatus,
                    topBlogsByComment
                }
            });
        }

        [HttpGet("admin-dashboard")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetAdminDashboardStats([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // I. Người dùng
            var users = await _context.User.ToListAsync();

            var userByRole = users
                .GroupBy(u => u.Role)
                .Select(g => new { role = g.Key, count = g.Count() })
                .ToList();

            var usersOverTime = users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
                .OrderBy(g => g.date)
                .ToList();

            var topStudents = users
                .Where(u => u.Role == 0)
                .OrderByDescending(u => u.ProblemSolved)
                .Take(3)
                .Select(u => new { u.FullName, u.ProblemSolved })
                .ToList();

            var topProfessors = users
                .Where(u => u.Role == 1)
                .OrderByDescending(u => u.ProblemCreated)
                .Take(3)
                .Select(u => new { u.FullName, u.ProblemCreated })
                .ToList();

            // II. Blog
            var blogs = await _context.BlogPost.ToListAsync();

            var blogByStatus = blogs
                .GroupBy(b => b.ApprovalStatus)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToList();

            var topBloggers = blogs
                .GroupBy(b => b.UserId)
                .Select(g => new {
                    userId = g.Key,
                    count = g.Count(),
                    name = _context.User.FirstOrDefault(u => u.UserId == g.Key)?.FullName ?? "Unknown"
                })
                .OrderByDescending(x => x.count)
                .Take(3)
                .ToList();

            var commentByBlog = _context.BlogComment
                .GroupBy(c => c.PostId)
                .Select(g => new {
                    blogTitle = _context.BlogPost.FirstOrDefault(b => b.BlogPostId == g.Key).Title ?? "Unknown",
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToList();

            // III. Problem
            var problems = await _context.Problem.ToListAsync();

            var problemByDifficulty = problems
                .GroupBy(p => p.Difficulty)
                .Select(g => new { difficulty = g.Key, count = g.Count() })
                .ToList();

            var topProblems = _context.Submission
                .GroupBy(s => s.ProblemId)
                .Select(g => new {
                    problemName = _context.Problem.FirstOrDefault(p => p.ProblemId == g.Key).Name ?? "Unknown",
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(3)
                .ToList();

            var judgeDistribution = _context.Submission
                .GroupBy(s => s.Result)
                .Select(g => new { result = g.Key, count = g.Count() })
                .ToList();

            var submissionOverTime = _context.Submission
                .Where(s => s.SubmitAtTime >= startDate && s.SubmitAtTime <= endDate)
                .AsEnumerable() // chuyển sang LINQ to Object từ đây
                .GroupBy(s => s.SubmitAtTime.Date)
                .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
                .OrderBy(g => g.date)
                .ToList();


            // IV. Learning Path
            var sections = await _context.Sections.ToListAsync();
            var modules = await _context.Modules.ToListAsync();
            var contents = await _context.ModuleContent.ToListAsync();

            var lessonBySection = contents
                .Join(modules, mc => mc.ModuleId, m => m.Id, (mc, m) => new { mc, m.SectionId })
                .Join(sections, mm => mm.SectionId, s => s.Id, (mm, s) => s.Name)
                .GroupBy(name => name)
                .Select(g => new { section = g.Key, count = g.Count() })
                .ToList();

            var lessonByFrequent = contents
                .GroupBy(c => c.Frequent)
                .Select(g => new { frequent = g.Key, count = g.Count() })
                .ToList();

            var topAuthors = contents
                .GroupBy(c => c.AuthorId)
                .Select(g => new {
                    author = _context.User.FirstOrDefault(u => u.UserId == g.Key)?.FullName ?? "Unknown",
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(3)
                .ToList();

            return Ok(new
            {
                user = new
                {
                    total = users.Count,
                    byRole = userByRole,
                    usersOverTime,
                    topStudents,
                    topProfessors
                },
                blog = new
                {
                    total = blogs.Count,
                    blogByStatus,
                    topBloggers,
                    commentByBlog
                },
                problem = new
                {
                    total = problems.Count,
                    problemByDifficulty,
                    topProblems,
                    judgeDistribution,
                    submissionOverTime
                },
                learning = new
                {
                    totalSections = sections.Count,
                    totalModules = modules.Count,
                    totalModuleContents = contents.Count,
                    lessonBySection,
                    lessonByFrequent,
                    topAuthors
                }
            });
        }












    }
}
