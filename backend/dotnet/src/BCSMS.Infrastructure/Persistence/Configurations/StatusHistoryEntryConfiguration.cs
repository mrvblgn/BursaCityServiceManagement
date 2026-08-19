using BCSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BCSMS.Infrastructure.Persistence.Configurations;

public class StatusHistoryEntryConfiguration : IEntityTypeConfiguration<StatusHistoryEntry>
{
    public void Configure(EntityTypeBuilder<StatusHistoryEntry> builder)
    {
        builder.ToTable("StatusHistoryEntries");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.ServiceRequestId)
            .IsRequired();

        builder.Property(h => h.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(h => h.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(h => h.Note)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(h => h.ChangedByUserId)
            .IsRequired();

        builder.Property(h => h.ChangedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
