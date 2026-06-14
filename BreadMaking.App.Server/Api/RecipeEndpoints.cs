using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Api;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes");

        // GET /api/recipes — list user-defined recipes only
        group.MapGet("/", async (AppDbContext db) =>
        {
            var recipes = await db.Recipes
                .Where(r => r.IsUserDefined)
                .Include(r => r.Formula)
                .OrderByDescending(r => r.Id)
                .Select(r => ToDto(r))
                .ToListAsync();
            return Results.Ok(recipes);
        });

        // POST /api/recipes — create a new user-defined recipe
        group.MapPost("/", async (SaveRecipeRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Name is required." });

            var recipe = new Recipe
            {
                Name          = req.Name.Trim(),
                GrainProfileId = null,
                Method        = req.Method == "Fermentolyse" ? BakeMethod.Fermentolyse : BakeMethod.Autolyse,
                TargetHydrationPct = req.HydrationPct,
                TargetDoughTempC   = 25,
                FrictionFactorC    = 4,
                IsUserDefined  = true,
                CreatedByLabel = req.GrainName,
            };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();

            var formula = new RecipeFormula
            {
                RecipeId     = recipe.Id,
                FlourWeightG = req.FlourWeightG,
                WaterPct     = req.HydrationPct,
                SaltPct      = req.SaltPct,
                StarterPct   = req.StarterPct,
                Notes        = req.Notes,
            };
            db.RecipeFormulas.Add(formula);
            await db.SaveChangesAsync();

            recipe.Formula = formula;
            var dto = ToDto(recipe);
            return Results.Created($"/api/recipes/{recipe.Id}", dto);
        });

        // PUT /api/recipes/{id} — update a user-defined recipe
        group.MapPut("/{id:int}", async (int id, UpdateRecipeRequest req, AppDbContext db) =>
        {
            var recipe = await db.Recipes
                .Include(r => r.Formula)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsUserDefined);
            if (recipe is null) return Results.NotFound();

            recipe.Name = req.Name.Trim();
            recipe.TargetHydrationPct = req.HydrationPct;

            if (recipe.Formula is null)
            {
                recipe.Formula = new RecipeFormula { RecipeId = recipe.Id };
                db.RecipeFormulas.Add(recipe.Formula);
            }
            recipe.Formula.FlourWeightG = req.FlourWeightG;
            recipe.Formula.WaterPct     = req.HydrationPct;
            recipe.Formula.SaltPct      = req.SaltPct;
            recipe.Formula.StarterPct   = req.StarterPct;
            recipe.Formula.Notes        = req.Notes;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /api/recipes/{id}
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.IsUserDefined);
            if (recipe is null) return Results.NotFound();
            db.Recipes.Remove(recipe);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static UserRecipeDto ToDto(Recipe r) => new()
    {
        Id           = r.Id,
        Name         = r.Name,
        GrainName    = r.CreatedByLabel ?? "",
        Method       = r.Method.ToString(),
        HydrationPct = r.TargetHydrationPct,
        FlourWeightG = r.Formula?.FlourWeightG ?? 0,
        SaltPct      = r.Formula?.SaltPct      ?? 0,
        StarterPct   = r.Formula?.StarterPct   ?? 0,
        Notes        = r.Formula?.Notes,
    };
}
