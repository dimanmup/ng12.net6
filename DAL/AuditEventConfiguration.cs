using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("ST__AUDIT");

        builder.Property(b => b.Id)
            .HasColumnName("ID");

        builder.Property(b => b.UtcDateTime)
            .HasColumnName("UTC_DT")
            .IsRequired(true);

        builder.Property(b => b.HttpStatusCode)
            .HasColumnName("HTTP_STATUS_CODE")
            .IsRequired(true)
            .HasDefaultValue(200);

        builder.Property(b => b.Description)
            .HasColumnName("DESCRIPTION")
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(b => b.Object)
            .HasColumnName("OBJECT")
            .IsRequired(true)
            .HasMaxLength(1000);

        builder.Property(b => b.Source)
            .HasColumnName("SOURCE")
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(b => b.SourceIPAddress)
            .HasColumnName("SOURCE_IP")
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(b => b.SourceDevice)
            .HasColumnName("SOURCE_DEVICE")
            .IsRequired(false)
            .HasMaxLength(100);
    }
}
