using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedRatioToStarterFeedLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedRatio",
                table: "StarterFeedLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedRatio",
                table: "StarterFeedLogs");
        }
    }
}
