using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddStarterJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StarterFeedLogId",
                table: "Bakes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Starters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    HydrationPct = table.Column<double>(type: "REAL", nullable: false),
                    FlourBlend = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StarterFeedLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StarterId = table.Column<int>(type: "INTEGER", nullable: false),
                    FedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FlourGrams = table.Column<double>(type: "REAL", nullable: false),
                    WaterGrams = table.Column<double>(type: "REAL", nullable: false),
                    PrevStarterGrams = table.Column<double>(type: "REAL", nullable: false),
                    AmbientTempC = table.Column<double>(type: "REAL", nullable: true),
                    PeakHours = table.Column<double>(type: "REAL", nullable: true),
                    FloatTestPassed = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarterFeedLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarterFeedLogs_Starters_StarterId",
                        column: x => x.StarterId,
                        principalTable: "Starters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bakes_StarterFeedLogId",
                table: "Bakes",
                column: "StarterFeedLogId");

            migrationBuilder.CreateIndex(
                name: "IX_StarterFeedLogs_StarterId",
                table: "StarterFeedLogs",
                column: "StarterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bakes_StarterFeedLogs_StarterFeedLogId",
                table: "Bakes",
                column: "StarterFeedLogId",
                principalTable: "StarterFeedLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bakes_StarterFeedLogs_StarterFeedLogId",
                table: "Bakes");

            migrationBuilder.DropTable(
                name: "StarterFeedLogs");

            migrationBuilder.DropTable(
                name: "Starters");

            migrationBuilder.DropIndex(
                name: "IX_Bakes_StarterFeedLogId",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "StarterFeedLogId",
                table: "Bakes");
        }
    }
}
