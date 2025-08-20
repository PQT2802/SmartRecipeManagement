using System.ComponentModel.DataAnnotations;

namespace SmartRecipe.Domain.Entities
{
    public class Step
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public int Order { get; set; } // For step sequence

        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}