using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Infrastructure.DB.ConfigData
{
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Description)
                .HasMaxLength(2000);

            builder.Property(r => r.Category)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasOne(r => r.CreatedByUser)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Ingredients)
                .WithOne(i => i.Recipe)
                .HasForeignKey(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Steps)
                .WithOne(s => s.Recipe)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.NutritionInfo)
                .WithOne(n => n.Recipe)
                .HasForeignKey<NutritionInfo>(n => n.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Comments)
                .WithOne(c => c.Recipe)
                .HasForeignKey(c => c.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Likes)
                .WithOne(l => l.Recipe)
                .HasForeignKey(l => l.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.MediaFiles)
                .WithOne(m => m.Recipe)
                .HasForeignKey(m => m.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}