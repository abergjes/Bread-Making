using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDefinedToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByLabel",
                table: "Recipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserDefined",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RecipeFormulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    FlourWeightG = table.Column<double>(type: "REAL", nullable: false),
                    WaterPct = table.Column<double>(type: "REAL", nullable: false),
                    SaltPct = table.Column<double>(type: "REAL", nullable: false),
                    StarterPct = table.Column<double>(type: "REAL", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeFormulas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeFormulas_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedByLabel", "IsUserDefined" },
                values: new object[] { null, false });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeFormulas_RecipeId",
                table: "RecipeFormulas",
                column: "RecipeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeFormulas");

            migrationBuilder.DropColumn(
                name: "CreatedByLabel",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "IsUserDefined",
                table: "Recipes");
        }
    }
}
