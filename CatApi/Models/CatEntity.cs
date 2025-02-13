using System.ComponentModel.DataAnnotations;

namespace CatApi.Models
{
    public class CatEntity
    {
        public int Id { get; set; }

        [Required]
        public string CatId { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Width must be a positive number.")]
        public int Width { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Height must be a positive number.")]

        public int Height { get; set; }

        public string ImagePath { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public List<TagEntity> Tags { get; set; } = new();
    }
}
