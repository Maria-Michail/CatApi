using CatApi.Models;

namespace CatApi.Services
{
    public interface ICatService
    {
        Task<string> FetchCatsAsync();
        Task<CatEntity?> GetCatByIdAsync(int id);
        Task<(int totalCount, List<CatEntity> cats)> GetCatsAsync(string? tag, int page, int pageSize);
    }
}
