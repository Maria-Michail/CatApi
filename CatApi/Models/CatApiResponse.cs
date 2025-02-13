namespace CatApi.Models
{
    public class CatApiResponse
    {
        public string Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
        public List<Breed> Breeds { get; set; }
    }

    public class Breed
    {
        public string Temperament { get; set; }
    }
}
