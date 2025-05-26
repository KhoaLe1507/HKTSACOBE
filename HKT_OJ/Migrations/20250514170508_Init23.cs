using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HKT_OJ.Migrations
{
    /// <inheritdoc />
    public partial class Init23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserSourceCode",
                table: "Submission",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserSourceCode",
                table: "Submission");
        }
    }
}
