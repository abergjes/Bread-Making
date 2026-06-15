using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBreadFormulaBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BreadFormulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreadFormulas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormulaIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormulaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Percent = table.Column<decimal>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaIngredients_BreadFormulas_FormulaId",
                        column: x => x.FormulaId,
                        principalTable: "BreadFormulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulaIngredients_FormulaId",
                table: "FormulaIngredients",
                column: "FormulaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulaIngredients");

            migrationBuilder.DropTable(
                name: "BreadFormulas");
        }
    }
}
