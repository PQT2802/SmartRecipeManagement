using System.ComponentModel.DataAnnotations;

namespace SmartRecipe.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}