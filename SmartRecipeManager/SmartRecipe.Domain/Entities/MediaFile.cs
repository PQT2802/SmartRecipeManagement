using SmartRecipe.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartRecipe.Domain.Entities
{
    public class MediaFile
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; }

        [Required]
        public MediaType FileType { get; set; } // e.g., Image, Video

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}