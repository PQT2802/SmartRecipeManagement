using System.ComponentModel.DataAnnotations;

namespace SmartRecipe.Domain.Entities
{
    public class Ingredient
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Quantity { get; set; }

        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}