using BCSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BCSMS.Infrastructure.Persistence.Configurations;

public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("ServiceRequests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(r => r.CategoryId)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Priority)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired(false);

        builder.OwnsOne(r => r.Location, locBuilder =>
        {
            locBuilder.Property(l => l.Latitude)
                .HasColumnName("Latitude")
                .IsRequired();

            locBuilder.Property(l => l.Longitude)
                .HasColumnName("Longitude")
                .IsRequired();

            locBuilder.Property(l => l.AddressText)
                .HasColumnName("AddressText")
                .HasMaxLength(500)
                .IsRequired(false);
        });

        builder.Property(r => r.CitizenId)
            .IsRequired();

        builder.Property(r => r.AssignedDepartmentId)
            .IsRequired(false);

        builder.Property(r => r.AssignedEmployeeId)
            .IsRequired(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Cross-aggregate relationships (Restrict on delete)
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(r => r.AssignedDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Child collections (Cascade on delete)
        builder.HasMany(r => r.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.Comments)
            .WithOne()
            .HasForeignKey(c => c.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.Attachments)
            .WithOne()
            .HasForeignKey(a => a.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Attachments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Query performance indexes
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CategoryId);
        builder.HasIndex(r => r.CitizenId);
        builder.HasIndex(r => r.AssignedDepartmentId);
        builder.HasIndex(r => r.AssignedEmployeeId);
        builder.HasIndex(r => r.CreatedAt);
    }
}
