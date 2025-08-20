using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Infrastructure.DB.ConfigData
{
    public class StepConfiguration : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.Order)
                .IsRequired();

            builder.HasOne(s => s.Recipe)
                .WithMany(r => r.Steps)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}