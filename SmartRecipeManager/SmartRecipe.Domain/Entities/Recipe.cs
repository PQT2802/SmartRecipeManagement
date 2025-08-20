using SmartRecipe.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRecipe.Domain.Entities
{
    public class Recipe
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public RecipeCategory Category { get; set; }

        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int Servings { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        // Navigation properties
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public ICollection<Step> Steps { get; set; } = new List<Step>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public NutritionInfo NutritionInfo { get; set; }

        // Computed properties (not mapped to database)
        [NotMapped]
        public int LikesCount => Likes?.Count ?? 0;

        [NotMapped]
        public int CommentsCount => Comments?.Count ?? 0;

        [NotMapped]
        public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
    }
}