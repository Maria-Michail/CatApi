using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CatApi.Models
{
    public class TagEntity
    {
        public int Id { get; set; }
        [Required, StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters.")]
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<CatEntity> Cats { get; set; } = new List<CatEntity>();
    }
}
