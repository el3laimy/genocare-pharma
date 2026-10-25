using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenoCare.Infrastructure.Persistence;

public sealed class GenoCareDesignTimeDbContextFactory : IDesignTimeDbContextFactory<GenoCareDbContext>
{
    public GenoCareDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GenoCareDbContext>()
            .UseSqlite("Data Source=genocare-design-time.db")
            .Options;
        return new GenoCareDbContext(options);
    }
}
