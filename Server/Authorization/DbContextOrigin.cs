using Microsoft.EntityFrameworkCore;
using DAL;

namespace Server.Authorization;

public class DbContextOrigin
{
    public readonly string connectionString = "";
    public readonly string provider;

    public DbContextOrigin(string povider = "sqlite", string connectionString = "Data Source=test.db; foreign keys=true;")
    {
        this.provider = povider;
        this.connectionString = connectionString;
    }

    public AppDbContext GetDbContext()
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        DbContextOptions<AppDbContext> options;
        
        switch (provider)
        {
            case "oracle":
                options = optionsBuilder.UseOracle(connectionString).Options;
                break;

            case "sqlite":
            default:
                options = optionsBuilder.UseSqlite(connectionString).Options;
                break;
        }

        return new AppDbContext(options);
    }
}
