using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesToBakeStepLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "BakeStepLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "BakeStepLogs");
        }
    }
}
