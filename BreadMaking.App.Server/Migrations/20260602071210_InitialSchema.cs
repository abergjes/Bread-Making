using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrainProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Ploidy = table.Column<string>(type: "TEXT", nullable: true),
                    GlutenStrength = table.Column<string>(type: "TEXT", nullable: false),
                    HydrationAdjustPct = table.Column<double>(type: "REAL", nullable: false),
                    MaxAutolyseMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    NeedsBinder = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrainProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    MinValid = table.Column<double>(type: "REAL", nullable: true),
                    MaxValid = table.Column<double>(type: "REAL", nullable: true),
                    DefaultForPhase = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: false),
                    GrainProfileId = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetHydrationPct = table.Column<double>(type: "REAL", nullable: false),
                    TargetDoughTempC = table.Column<double>(type: "REAL", nullable: false),
                    FrictionFactorC = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_GrainProfiles_GrainProfileId",
                        column: x => x.GrainProfileId,
                        principalTable: "GrainProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    AmbientTempC = table.Column<double>(type: "REAL", nullable: true),
                    AmbientHumidityPct = table.Column<double>(type: "REAL", nullable: true),
                    FlourBatch = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bakes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phase = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultDurationMin = table.Column<int>(type: "INTEGER", nullable: false),
                    MinDurationMin = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDurationMin = table.Column<int>(type: "INTEGER", nullable: false),
                    StepMin = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetTempC = table.Column<double>(type: "REAL", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BakeOutcomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BakeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoafHeightCm = table.Column<double>(type: "REAL", nullable: true),
                    OvenSpringPct = table.Column<double>(type: "REAL", nullable: true),
                    InternalTempC = table.Column<double>(type: "REAL", nullable: true),
                    WeightLossPct = table.Column<double>(type: "REAL", nullable: true),
                    CrumbOpenness = table.Column<int>(type: "INTEGER", nullable: true),
                    CrustScore = table.Column<int>(type: "INTEGER", nullable: true),
                    TasteScore = table.Column<int>(type: "INTEGER", nullable: true),
                    PhotoPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakeOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BakeOutcomes_Bakes_BakeId",
                        column: x => x.BakeId,
                        principalTable: "Bakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BakeStepLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BakeId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipeStepId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedDurationMin = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakeStepLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BakeStepLogs_Bakes_BakeId",
                        column: x => x.BakeId,
                        principalTable: "Bakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BakeStepLogs_RecipeSteps_RecipeStepId",
                        column: x => x.RecipeStepId,
                        principalTable: "RecipeSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BakeStepLogId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeasurementTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_BakeStepLogs_BakeStepLogId",
                        column: x => x.BakeStepLogId,
                        principalTable: "BakeStepLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Measurements_MeasurementTypes_MeasurementTypeId",
                        column: x => x.MeasurementTypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GrainProfiles",
                columns: new[] { "Id", "GlutenStrength", "HydrationAdjustPct", "MaxAutolyseMinutes", "Name", "NeedsBinder", "Ploidy" },
                values: new object[,]
                {
                    { 1, "Strong", 0.0, 60, "Modern wheat", false, "Hexaploid" },
                    { 2, "Strong", 5.0, 60, "Whole grain", false, "Hexaploid" },
                    { 3, "Very low (bran)", 15.0, 60, "Rye", false, null },
                    { 4, "Moderate (fragile)", -5.0, 30, "Spelt", false, "Hexaploid" },
                    { 5, "Very weak", -15.0, 15, "Einkorn", false, "Diploid" },
                    { 6, "Weak", -10.0, 20, "Emmer (farro)", false, "Tetraploid" },
                    { 7, "Strong", 10.0, 45, "Kamut (khorasan)", false, "Tetraploid" },
                    { 8, "None (GF)", 0.0, 0, "Teff", true, null },
                    { 9, "None (GF)", 0.0, 0, "Sorghum", true, null }
                });

            migrationBuilder.InsertData(
                table: "MeasurementTypes",
                columns: new[] { "Id", "Category", "DefaultForPhase", "MaxValid", "MinValid", "Name", "Unit" },
                values: new object[,]
                {
                    { 1, "InProcess", "Mix", 40.0, 10.0, "Dough temp", "°C" },
                    { 2, "InProcess", "Bulk", 200.0, 0.0, "Aliquot rise", "%" },
                    { 3, "InProcess", "Bulk", 7.0, 3.0, "pH", "pH" },
                    { 4, "InProcess", "Bulk", 30.0, 0.0, "TTA", "mL" },
                    { 5, "Outcome", "Bake", 110.0, 80.0, "Internal temp", "°C" }
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "FrictionFactorC", "GrainProfileId", "Method", "Name", "TargetDoughTempC", "TargetHydrationPct" },
                values: new object[,]
                {
                    { 1, 4.0, 1, 0, "Modern wheat — Autolyse", 25.0, 72.0 },
                    { 2, 4.0, 1, 1, "Modern wheat — Fermentolyse", 25.0, 72.0 },
                    { 3, 4.0, 4, 0, "Spelt — Autolyse", 24.0, 68.0 },
                    { 4, 4.0, 4, 1, "Spelt — Fermentolyse", 24.0, 68.0 },
                    { 5, 4.0, 5, 0, "Einkorn — Autolyse", 24.0, 62.0 },
                    { 6, 4.0, 5, 1, "Einkorn — Fermentolyse", 24.0, 62.0 },
                    { 7, 4.0, 6, 0, "Emmer — Autolyse", 24.0, 65.0 },
                    { 8, 4.0, 6, 1, "Emmer — Fermentolyse", 24.0, 65.0 },
                    { 9, 4.0, 7, 0, "Kamut — Autolyse", 25.0, 78.0 },
                    { 10, 4.0, 7, 1, "Kamut — Fermentolyse", 25.0, 78.0 },
                    { 11, 0.0, 8, 2, "Teff — Soaker", 24.0, 95.0 },
                    { 12, 0.0, 9, 2, "Sorghum — Soaker", 24.0, 90.0 }
                });

            migrationBuilder.InsertData(
                table: "RecipeSteps",
                columns: new[] { "Id", "DefaultDurationMin", "Description", "MaxDurationMin", "MinDurationMin", "Name", "Order", "Phase", "RecipeId", "StepMin", "TargetTempC" },
                values: new object[,]
                {
                    { 101, 5, "Rough mix of flour and water only — no salt, no starter yet.", 15, 3, "Mix flour + water", 1, "Mix", 1, 5, null },
                    { 102, 50, "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten.", 60, 40, "Autolyse rest", 2, "Rest", 1, 5, null },
                    { 103, 5, "Dimple salt in and fold to incorporate. Add starter and fold in fully.", 15, 3, "Add salt + starter", 3, "Mix", 1, 5, null },
                    { 104, 300, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 360, 240, "Bulk fermentation", 4, "Bulk", 1, 15, 23.0 },
                    { 105, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 1, 5, null },
                    { 106, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 1, 5, null },
                    { 107, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 1, 5, null },
                    { 108, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 1, 60, 4.0 },
                    { 109, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 1, 15, 250.0 },
                    { 110, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 1, 5, 250.0 },
                    { 111, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 1, 5, 220.0 },
                    { 112, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 1, 30, null },
                    { 201, 5, "Rough mix of flour, water, and starter — no salt yet.", 15, 3, "Mix flour + water + starter", 1, "Mix", 2, 5, null },
                    { 202, 75, "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens.", 90, 60, "Fermentolyse rest", 2, "Rest", 2, 5, null },
                    { 203, 5, "Dimple salt in and fold to incorporate thoroughly.", 15, 3, "Add salt", 3, "Mix", 2, 5, null },
                    { 204, 300, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 360, 240, "Bulk fermentation", 4, "Bulk", 2, 15, 23.0 },
                    { 205, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 2, 5, null },
                    { 206, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 2, 5, null },
                    { 207, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 2, 5, null },
                    { 208, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 2, 60, 4.0 },
                    { 209, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 2, 15, 250.0 },
                    { 210, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 2, 5, 250.0 },
                    { 211, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 2, 5, 220.0 },
                    { 212, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 2, 30, null },
                    { 301, 5, "Rough mix of flour and water only — no salt, no starter yet.", 15, 3, "Mix flour + water", 1, "Mix", 3, 5, null },
                    { 302, 20, "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten.", 30, 15, "Autolyse rest", 2, "Rest", 3, 5, null },
                    { 303, 5, "Dimple salt in and fold to incorporate. Add starter and fold in fully.", 15, 3, "Add salt + starter", 3, "Mix", 3, 5, null },
                    { 304, 240, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 300, 180, "Bulk fermentation", 4, "Bulk", 3, 15, 23.0 },
                    { 305, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 3, 5, null },
                    { 306, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 3, 5, null },
                    { 307, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 3, 5, null },
                    { 308, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 3, 60, 4.0 },
                    { 309, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 3, 15, 250.0 },
                    { 310, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 3, 5, 250.0 },
                    { 311, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 3, 5, 220.0 },
                    { 312, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 3, 30, null },
                    { 401, 5, "Rough mix of flour, water, and starter — no salt yet.", 15, 3, "Mix flour + water + starter", 1, "Mix", 4, 5, null },
                    { 402, 25, "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens.", 30, 20, "Fermentolyse rest", 2, "Rest", 4, 5, null },
                    { 403, 5, "Dimple salt in and fold to incorporate thoroughly.", 15, 3, "Add salt", 3, "Mix", 4, 5, null },
                    { 404, 240, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 300, 180, "Bulk fermentation", 4, "Bulk", 4, 15, 23.0 },
                    { 405, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 4, 5, null },
                    { 406, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 4, 5, null },
                    { 407, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 4, 5, null },
                    { 408, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 4, 60, 4.0 },
                    { 409, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 4, 15, 250.0 },
                    { 410, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 4, 5, 250.0 },
                    { 411, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 4, 5, 220.0 },
                    { 412, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 4, 30, null },
                    { 501, 5, "Rough mix of flour and water only — no salt, no starter yet.", 15, 3, "Mix flour + water", 1, "Mix", 5, 5, null },
                    { 502, 10, "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten.", 15, 5, "Autolyse rest", 2, "Rest", 5, 5, null },
                    { 503, 5, "Dimple salt in and fold to incorporate. Add starter and fold in fully.", 15, 3, "Add salt + starter", 3, "Mix", 5, 5, null },
                    { 504, 180, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 240, 120, "Bulk fermentation", 4, "Bulk", 5, 15, 23.0 },
                    { 505, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 5, 5, null },
                    { 506, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 5, 5, null },
                    { 507, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 5, 5, null },
                    { 508, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 5, 60, 4.0 },
                    { 509, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 5, 15, 250.0 },
                    { 510, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 5, 5, 250.0 },
                    { 511, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 5, 5, 220.0 },
                    { 512, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 5, 30, null },
                    { 601, 5, "Rough mix of flour, water, and starter — no salt yet.", 15, 3, "Mix flour + water + starter", 1, "Mix", 6, 5, null },
                    { 602, 13, "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens.", 15, 10, "Fermentolyse rest", 2, "Rest", 6, 5, null },
                    { 603, 5, "Dimple salt in and fold to incorporate thoroughly.", 15, 3, "Add salt", 3, "Mix", 6, 5, null },
                    { 604, 180, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 240, 120, "Bulk fermentation", 4, "Bulk", 6, 15, 23.0 },
                    { 605, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 6, 5, null },
                    { 606, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 6, 5, null },
                    { 607, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 6, 5, null },
                    { 608, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 6, 60, 4.0 },
                    { 609, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 6, 15, 250.0 },
                    { 610, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 6, 5, 250.0 },
                    { 611, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 6, 5, 220.0 },
                    { 612, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 6, 30, null },
                    { 701, 5, "Rough mix of flour and water only — no salt, no starter yet.", 15, 3, "Mix flour + water", 1, "Mix", 7, 5, null },
                    { 702, 15, "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten.", 20, 10, "Autolyse rest", 2, "Rest", 7, 5, null },
                    { 703, 5, "Dimple salt in and fold to incorporate. Add starter and fold in fully.", 15, 3, "Add salt + starter", 3, "Mix", 7, 5, null },
                    { 704, 210, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 270, 150, "Bulk fermentation", 4, "Bulk", 7, 15, 23.0 },
                    { 705, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 7, 5, null },
                    { 706, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 7, 5, null },
                    { 707, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 7, 5, null },
                    { 708, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 7, 60, 4.0 },
                    { 709, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 7, 15, 250.0 },
                    { 710, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 7, 5, 250.0 },
                    { 711, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 7, 5, 220.0 },
                    { 712, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 7, 30, null },
                    { 801, 5, "Rough mix of flour, water, and starter — no salt yet.", 15, 3, "Mix flour + water + starter", 1, "Mix", 8, 5, null },
                    { 802, 18, "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens.", 20, 15, "Fermentolyse rest", 2, "Rest", 8, 5, null },
                    { 803, 5, "Dimple salt in and fold to incorporate thoroughly.", 15, 3, "Add salt", 3, "Mix", 8, 5, null },
                    { 804, 210, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 270, 150, "Bulk fermentation", 4, "Bulk", 8, 15, 23.0 },
                    { 805, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 8, 5, null },
                    { 806, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 8, 5, null },
                    { 807, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 8, 5, null },
                    { 808, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 8, 60, 4.0 },
                    { 809, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 8, 15, 250.0 },
                    { 810, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 8, 5, 250.0 },
                    { 811, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 8, 5, 220.0 },
                    { 812, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 8, 30, null },
                    { 901, 5, "Rough mix of flour and water only — no salt, no starter yet.", 15, 3, "Mix flour + water", 1, "Mix", 9, 5, null },
                    { 902, 37, "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten.", 45, 30, "Autolyse rest", 2, "Rest", 9, 5, null },
                    { 903, 5, "Dimple salt in and fold to incorporate. Add starter and fold in fully.", 15, 3, "Add salt + starter", 3, "Mix", 9, 5, null },
                    { 904, 330, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 420, 270, "Bulk fermentation", 4, "Bulk", 9, 15, 23.0 },
                    { 905, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 9, 5, null },
                    { 906, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 9, 5, null },
                    { 907, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 9, 5, null },
                    { 908, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 9, 60, 4.0 },
                    { 909, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 9, 15, 250.0 },
                    { 910, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 9, 5, 250.0 },
                    { 911, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 9, 5, 220.0 },
                    { 912, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 9, 30, null },
                    { 1001, 5, "Rough mix of flour, water, and starter — no salt yet.", 15, 3, "Mix flour + water + starter", 1, "Mix", 10, 5, null },
                    { 1002, 40, "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens.", 45, 35, "Fermentolyse rest", 2, "Rest", 10, 5, null },
                    { 1003, 5, "Dimple salt in and fold to incorporate thoroughly.", 15, 3, "Add salt", 3, "Mix", 10, 5, null },
                    { 1004, 330, "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%.", 420, 270, "Bulk fermentation", 4, "Bulk", 10, 15, 23.0 },
                    { 1005, 10, "Gentle pre-shape. Flour the bench lightly.", 20, 5, "Pre-shape", 5, "Shape", 10, 5, null },
                    { 1006, 30, "Leave uncovered on bench. Let gluten relax before final shape.", 45, 20, "Bench rest", 6, "Shape", 10, 5, null },
                    { 1007, 10, "Build surface tension. Place seam-side up in floured banneton.", 20, 5, "Final shape", 7, "Shape", 10, 5, null },
                    { 1008, 960, "Retard in fridge overnight. Develops flavour and structure.", 1440, 480, "Cold proof", 8, "Proof", 10, 60, 4.0 },
                    { 1009, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 10, 15, 250.0 },
                    { 1010, 20, "Steam inside Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 10, 5, 250.0 },
                    { 1011, 20, "Achieve deep caramelised crust. Internal temp should reach 96–98 °C.", 30, 15, "Bake uncovered", 11, "Bake", 10, 5, 220.0 },
                    { 1012, 120, "Crumb is still setting during cooling — do not cut early.", 180, 60, "Cool on rack", 12, "Cool", 10, 30, null },
                    { 1101, 5, "Combine flour, liquid, and binder (psyllium / xanthan / egg). No kneading.", 10, 3, "Whisk batter", 1, "Mix", 11, 5, null },
                    { 1102, 40, "Cover and rest. Starch absorbs liquid — reduces grittiness, improves crumb texture.", 60, 30, "Soaker rest", 2, "Rest", 11, 5, null },
                    { 1103, 5, "Fold in salt, any sweetener, fat, and starter if using sourdough.", 10, 3, "Add remaining ingredients", 3, "Mix", 11, 5, null },
                    { 1104, 90, "Gluten-free batter rises quickly and less dramatically. Watch for bubbles.", 180, 60, "Ferment / proof", 4, "Proof", 11, 15, 25.0 },
                    { 1105, 20, "Cover with foil or lid to trap steam and prevent early over-crust.", 25, 15, "Bake in tin (covered)", 5, "Bake", 11, 5, 220.0 },
                    { 1106, 30, "Until crust is set and internal temperature reaches 96–98 °C.", 40, 25, "Bake uncovered", 6, "Bake", 11, 5, 200.0 },
                    { 1107, 90, "Gluten-free crumb continues setting during cooling — do not cut early.", 120, 60, "Cool on rack", 7, "Cool", 11, 15, null },
                    { 1201, 5, "Combine flour, liquid, and binder (psyllium / xanthan / egg). No kneading.", 10, 3, "Whisk batter", 1, "Mix", 12, 5, null },
                    { 1202, 45, "Cover and rest. Starch absorbs liquid — reduces grittiness, improves crumb texture.", 60, 30, "Soaker rest", 2, "Rest", 12, 5, null },
                    { 1203, 5, "Fold in salt, any sweetener, fat, and starter if using sourdough.", 10, 3, "Add remaining ingredients", 3, "Mix", 12, 5, null },
                    { 1204, 90, "Gluten-free batter rises quickly and less dramatically. Watch for bubbles.", 180, 60, "Ferment / proof", 4, "Proof", 12, 15, 25.0 },
                    { 1205, 20, "Cover with foil or lid to trap steam and prevent early over-crust.", 25, 15, "Bake in tin (covered)", 5, "Bake", 12, 5, 220.0 },
                    { 1206, 30, "Until crust is set and internal temperature reaches 96–98 °C.", 40, 25, "Bake uncovered", 6, "Bake", 12, 5, 200.0 },
                    { 1207, 90, "Gluten-free crumb continues setting during cooling — do not cut early.", 120, 60, "Cool on rack", 7, "Cool", 12, 15, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BakeOutcomes_BakeId",
                table: "BakeOutcomes",
                column: "BakeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bakes_RecipeId",
                table: "Bakes",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_BakeStepLogs_BakeId",
                table: "BakeStepLogs",
                column: "BakeId");

            migrationBuilder.CreateIndex(
                name: "IX_BakeStepLogs_RecipeStepId",
                table: "BakeStepLogs",
                column: "RecipeStepId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_BakeStepLogId",
                table: "Measurements",
                column: "BakeStepLogId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_MeasurementTypeId",
                table: "Measurements",
                column: "MeasurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_GrainProfileId",
                table: "Recipes",
                column: "GrainProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BakeOutcomes");

            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "BakeStepLogs");

            migrationBuilder.DropTable(
                name: "MeasurementTypes");

            migrationBuilder.DropTable(
                name: "Bakes");

            migrationBuilder.DropTable(
                name: "RecipeSteps");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "GrainProfiles");
        }
    }
}
