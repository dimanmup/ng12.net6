using Microsoft.EntityFrameworkCore;

namespace DAL
{
    #nullable disable
    public class AppDbContext : DbContext
    {
        public DbSet<AuditEvent> AuditEvents { get; set; }

        // Чтобы context прилетал в конструктор контроллера.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {  
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AuditEventConfiguration());
        }
    }
}
