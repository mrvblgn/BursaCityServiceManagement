using BCSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BCSMS.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(d => d.Name)
            .IsUnique();
    }
}
