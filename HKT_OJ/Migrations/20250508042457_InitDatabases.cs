using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HKT_OJ.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Testcases",
                table: "Testcases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubmissionResults",
                table: "SubmissionResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sections",
                table: "Sections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Problems",
                table: "Problems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleContents",
                table: "ModuleContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentReactions",
                table: "CommentReactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogReactions",
                table: "BlogReactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPosts",
                table: "BlogPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogComments",
                table: "BlogComments");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Testcases",
                newName: "Testcase");

            migrationBuilder.RenameTable(
                name: "Submissions",
                newName: "Submission");

            migrationBuilder.RenameTable(
                name: "SubmissionResults",
                newName: "SubmissionResult");

            migrationBuilder.RenameTable(
                name: "Sections",
                newName: "Section");

            migrationBuilder.RenameTable(
                name: "Problems",
                newName: "Problem");

            migrationBuilder.RenameTable(
                name: "ModuleContents",
                newName: "ModuleContent");

            migrationBuilder.RenameTable(
                name: "CommentReactions",
                newName: "CommentReaction");

            migrationBuilder.RenameTable(
                name: "BlogReactions",
                newName: "BlogReaction");

            migrationBuilder.RenameTable(
                name: "BlogPosts",
                newName: "BlogPost");

            migrationBuilder.RenameTable(
                name: "BlogComments",
                newName: "BlogComment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Testcase",
                table: "Testcase",
                column: "TestcaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submission",
                table: "Submission",
                column: "SubmissionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubmissionResult",
                table: "SubmissionResult",
                column: "SubmissionResultId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Section",
                table: "Section",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Problem",
                table: "Problem",
                column: "ProblemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleContent",
                table: "ModuleContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentReaction",
                table: "CommentReaction",
                column: "CommentReactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogReaction",
                table: "BlogReaction",
                column: "BlogReactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPost",
                table: "BlogPost",
                column: "BlogPostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogComment",
                table: "BlogComment",
                column: "BlogCommentId");

            migrationBuilder.CreateTable(
                name: "ProblemConstraint",
                columns: table => new
                {
                    ProblemConstraintId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    Variable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinValue = table.Column<long>(type: "bigint", nullable: false),
                    MaxValue = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemConstraint", x => x.ProblemConstraintId);
                });

            migrationBuilder.CreateTable(
                name: "Solution",
                columns: table => new
                {
                    SolutionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solution", x => x.SolutionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProblemConstraint");

            migrationBuilder.DropTable(
                name: "Solution");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Testcase",
                table: "Testcase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubmissionResult",
                table: "SubmissionResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submission",
                table: "Submission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Section",
                table: "Section");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Problem",
                table: "Problem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleContent",
                table: "ModuleContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentReaction",
                table: "CommentReaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogReaction",
                table: "BlogReaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPost",
                table: "BlogPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogComment",
                table: "BlogComment");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Testcase",
                newName: "Testcases");

            migrationBuilder.RenameTable(
                name: "SubmissionResult",
                newName: "SubmissionResults");

            migrationBuilder.RenameTable(
                name: "Submission",
                newName: "Submissions");

            migrationBuilder.RenameTable(
                name: "Section",
                newName: "Sections");

            migrationBuilder.RenameTable(
                name: "Problem",
                newName: "Problems");

            migrationBuilder.RenameTable(
                name: "ModuleContent",
                newName: "ModuleContents");

            migrationBuilder.RenameTable(
                name: "CommentReaction",
                newName: "CommentReactions");

            migrationBuilder.RenameTable(
                name: "BlogReaction",
                newName: "BlogReactions");

            migrationBuilder.RenameTable(
                name: "BlogPost",
                newName: "BlogPosts");

            migrationBuilder.RenameTable(
                name: "BlogComment",
                newName: "BlogComments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Testcases",
                table: "Testcases",
                column: "TestcaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubmissionResults",
                table: "SubmissionResults",
                column: "SubmissionResultId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions",
                column: "SubmissionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sections",
                table: "Sections",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Problems",
                table: "Problems",
                column: "ProblemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleContents",
                table: "ModuleContents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentReactions",
                table: "CommentReactions",
                column: "CommentReactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogReactions",
                table: "BlogReactions",
                column: "BlogReactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPosts",
                table: "BlogPosts",
                column: "BlogPostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogComments",
                table: "BlogComments",
                column: "BlogCommentId");
        }
    }
}
