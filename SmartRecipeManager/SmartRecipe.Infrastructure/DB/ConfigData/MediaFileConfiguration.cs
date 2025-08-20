using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Infrastructure.DB.ConfigData
{
    public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
    {
        public void Configure(EntityTypeBuilder<MediaFile> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(m => m.FileUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(m => m.FileType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.UploadedAt)
                .IsRequired();

            builder.HasOne(m => m.Recipe)
                .WithMany(r => r.MediaFiles)
                .HasForeignKey(m => m.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}