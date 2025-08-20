using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Infrastructure.DB.ConfigData
{
    public class NutritionInfoConfiguration : IEntityTypeConfiguration<NutritionInfo>
    {
        public void Configure(EntityTypeBuilder<NutritionInfo> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Calories)
                .IsRequired();

            builder.Property(n => n.Protein)
                .IsRequired();

            builder.Property(n => n.Fat)
                .IsRequired();

            builder.Property(n => n.Carbs)
                .IsRequired();

            builder.HasOne(n => n.Recipe)
                .WithOne(r => r.NutritionInfo)
                .HasForeignKey<NutritionInfo>(n => n.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}