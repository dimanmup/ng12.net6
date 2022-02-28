using Microsoft.EntityFrameworkCore;

namespace DAL
{
    #nullable disable
    public class AppDbContext : DbContext
    {
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public readonly FluentSettings fluentSettings;

        // Чтобы context прилетал в конструктор контроллера.
        public AppDbContext(DbContextOptions<AppDbContext> options, FluentSettings fluentSettings) : base(options)
        {  
            this.fluentSettings = fluentSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AuditEventConfiguration(fluentSettings));
        }
    }
}
