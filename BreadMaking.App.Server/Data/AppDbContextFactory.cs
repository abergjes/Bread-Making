using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BreadMaking.App.Server.Data;

// Lets `dotnet ef` create the context without running the full app startup.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=bread-making.db")
            .Options;
        return new AppDbContext(options);
    }
}
