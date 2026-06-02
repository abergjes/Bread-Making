using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GrainProfileEntity> GrainProfiles => Set<GrainProfileEntity>();
    public DbSet<Recipe>             Recipes        => Set<Recipe>();
    public DbSet<RecipeStep>         RecipeSteps    => Set<RecipeStep>();
    public DbSet<Bake>               Bakes          => Set<Bake>();
    public DbSet<BakeStepLog>        BakeStepLogs   => Set<BakeStepLog>();
    public DbSet<MeasurementType>    MeasurementTypes => Set<MeasurementType>();
    public DbSet<Measurement>        Measurements   => Set<Measurement>();
    public DbSet<BakeOutcome>        BakeOutcomes   => Set<BakeOutcome>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BakeOutcome is 1:1 with Bake
        modelBuilder.Entity<BakeOutcome>()
            .HasOne(o => o.Bake)
            .WithOne(b => b.Outcome)
            .HasForeignKey<BakeOutcome>(o => o.BakeId);

        // GrainProfileEntity table name (avoid shadowing client model)
        modelBuilder.Entity<GrainProfileEntity>().ToTable("GrainProfiles");

        // Seed data
        modelBuilder.Entity<GrainProfileEntity>().HasData(SeedData.GrainProfiles);
        modelBuilder.Entity<Recipe>().HasData(SeedData.Recipes);
        modelBuilder.Entity<RecipeStep>().HasData(SeedData.RecipeSteps);
        modelBuilder.Entity<MeasurementType>().HasData(SeedData.MeasurementTypes);
    }
}
