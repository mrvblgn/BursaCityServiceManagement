using BCSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BCSMS.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.ServiceRequestId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.FileSizeInBytes)
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.UploadedByUserId)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
