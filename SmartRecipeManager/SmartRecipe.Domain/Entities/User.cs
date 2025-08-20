using SmartRecipe.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartRecipe.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public UserRole Role { get; set; } = UserRole.Viewer; // Admin, Contributor, Viewer

        [MaxLength(1000)]
        public string? ProfileImageUrl { get; set; }

        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}