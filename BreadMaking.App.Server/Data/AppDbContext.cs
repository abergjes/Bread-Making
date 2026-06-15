using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GrainProfileEntity> GrainProfiles   => Set<GrainProfileEntity>();
    public DbSet<Recipe>             Recipes          => Set<Recipe>();
    public DbSet<RecipeFormula>      RecipeFormulas   => Set<RecipeFormula>();
    public DbSet<RecipeStep>         RecipeSteps      => Set<RecipeStep>();
    public DbSet<Bake>               Bakes            => Set<Bake>();
    public DbSet<BakeStepLog>        BakeStepLogs     => Set<BakeStepLog>();
    public DbSet<MeasurementType>    MeasurementTypes => Set<MeasurementType>();
    public DbSet<Measurement>        Measurements     => Set<Measurement>();
    public DbSet<BakeOutcome>        BakeOutcomes     => Set<BakeOutcome>();
    public DbSet<Starter>            Starters         => Set<Starter>();
    public DbSet<StarterFeedLog>     StarterFeedLogs  => Set<StarterFeedLog>();
    public DbSet<BreadFormula>       BreadFormulas    => Set<BreadFormula>();
    public DbSet<FormulaIngredient>  FormulaIngredients => Set<FormulaIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BakeOutcome is 1:1 with Bake
        modelBuilder.Entity<BakeOutcome>()
            .HasOne(o => o.Bake)
            .WithOne(b => b.Outcome)
            .HasForeignKey<BakeOutcome>(o => o.BakeId);

        // GrainProfileEntity table name (avoid shadowing client model)
        modelBuilder.Entity<GrainProfileEntity>().ToTable("GrainProfiles");

        // Starter → StarterFeedLog (cascade delete)
        modelBuilder.Entity<StarterFeedLog>()
            .HasOne(f => f.Starter)
            .WithMany(s => s.Feeds)
            .HasForeignKey(f => f.StarterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bake → StarterFeedLog (nullable FK, set null on feed delete)
        modelBuilder.Entity<Bake>()
            .HasOne(b => b.StarterFeed)
            .WithMany()
            .HasForeignKey(b => b.StarterFeedLogId)
            .OnDelete(DeleteBehavior.SetNull);

        // RecipeFormula 1:1 with Recipe
        modelBuilder.Entity<RecipeFormula>()
            .HasOne(f => f.Recipe)
            .WithOne(r => r.Formula)
            .HasForeignKey<RecipeFormula>(f => f.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // FormulaIngredient → BreadFormula (cascade delete)
        modelBuilder.Entity<FormulaIngredient>()
            .HasOne(i => i.Formula)
            .WithMany(f => f.Ingredients)
            .HasForeignKey(i => i.FormulaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        modelBuilder.Entity<GrainProfileEntity>().HasData(SeedData.GrainProfiles);
        modelBuilder.Entity<Recipe>().HasData(SeedData.Recipes);
        modelBuilder.Entity<RecipeStep>().HasData(SeedData.RecipeSteps);
        modelBuilder.Entity<MeasurementType>().HasData(SeedData.MeasurementTypes);
    }
}
