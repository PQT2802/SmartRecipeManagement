using System.ComponentModel.DataAnnotations;

namespace SmartRecipe.Domain.Entities
{
    public class NutritionInfo
    {
        public Guid Id { get; set; }

        [Required]
        public decimal Calories { get; set; }

        [Required]
        public decimal Protein { get; set; }

        [Required]
        public decimal Fat { get; set; }

        [Required]
        public decimal Carbs { get; set; }

        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}