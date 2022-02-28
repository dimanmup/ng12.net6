using Microsoft.EntityFrameworkCore;
using DAL;

namespace Server.Authorization;

public class DbContextOrigin
{
    public readonly string ConnectionString = "";
    public readonly string Provider;
    public readonly FluentSettings FluentSettings;

    public DbContextOrigin(string povider = "SQLite", string connectionString = "Data Source=test.db; foreign keys=true;", FluentSettings? fluentSettings = null)
    {
        Provider = povider;
        ConnectionString = connectionString;
        FluentSettings = fluentSettings ?? new FluentSettings();
    }

    public AppDbContext GetDbContext()
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        DbContextOptions<AppDbContext> options;
        
        switch (Provider)
        {
            case "Oracle":
                options = optionsBuilder.UseOracle(ConnectionString).Options;
                break;

            case "PostgreSQL":
                options = optionsBuilder.UseNpgsql(ConnectionString).Options;
                break;

            case "SQLite":
            default:
                options = optionsBuilder.UseSqlite(ConnectionString).Options;
                break;
        }

        return new AppDbContext(options, FluentSettings);
    }
}
