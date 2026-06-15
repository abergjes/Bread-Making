using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Api;

public static class FormulaEndpoints
{
    public static IEndpointRouteBuilder MapFormulaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/formulas");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var formulas = await db.BreadFormulas
                .Include(f => f.Ingredients.OrderBy(i => i.SortOrder))
                .OrderByDescending(f => f.Id)
                .ToListAsync();
            return Results.Ok(formulas.Select(ToDto));
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var formula = await db.BreadFormulas
                .Include(f => f.Ingredients.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(f => f.Id == id);
            return formula is null ? Results.NotFound() : Results.Ok(ToDto(formula));
        });

        group.MapPost("/", async (SaveFormulaRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest("Name is required.");

            var formula = new BreadFormula
            {
                Name      = req.Name.Trim(),
                Notes     = req.Notes?.Trim(),
                Tags      = req.Tags?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
                Ingredients = req.Ingredients
                    .Select((ing, idx) => new FormulaIngredient
                    {
                        Name      = ing.Name.Trim(),
                        Percent   = ing.Percent,
                        SortOrder = idx,
                    })
                    .ToList(),
            };

            db.BreadFormulas.Add(formula);
            await db.SaveChangesAsync();
            return Results.Created($"/api/formulas/{formula.Id}", ToDto(formula));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var formula = await db.BreadFormulas.FindAsync(id);
            if (formula is null) return Results.NotFound();
            db.BreadFormulas.Remove(formula);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static FormulaDto ToDto(BreadFormula f) => new()
    {
        Id        = f.Id,
        Name      = f.Name,
        Notes     = f.Notes,
        Tags      = f.Tags,
        CreatedAt = f.CreatedAt,
        Ingredients = f.Ingredients
            .OrderBy(i => i.SortOrder)
            .Select(i => new FormulaIngredientDto
            {
                Id        = i.Id,
                Name      = i.Name,
                Percent   = i.Percent,
                SortOrder = i.SortOrder,
            })
            .ToList(),
    };
}
