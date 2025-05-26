using Microsoft.EntityFrameworkCore;
using HKT_OJ.Models;
using static System.Collections.Specialized.BitVector32;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace HKT_OJ.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Problem> Problem { get; set; }
        public DbSet<ProblemConstraint> ProblemConstraint { get; set; }
        public DbSet<Submission> Submission { get; set; }
        public DbSet<SubmissionResult> SubmissionResult { get; set; }
        public DbSet<Solution> Solution { get; set; }
        public DbSet<Testcase> Testcase { get; set; }

        public DbSet<Sections> Sections { get; set; }
        public DbSet<Modules> Modules { get; set; }
        public DbSet<ModuleContent> ModuleContent { get; set; }

        public DbSet<BlogPost> BlogPost { get; set; }
        public DbSet<BlogComment> BlogComment { get; set; }
        public DbSet<BlogReaction> BlogReaction { get; set; }
        public DbSet<CommentReaction> CommentReaction { get; set; }

        public DbSet<ProblemProgress> ProblemProgress { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // các cấu hình Fluent API sẽ viết tại đây sau
        }
    }
}
