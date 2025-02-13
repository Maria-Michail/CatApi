using CatApi.Data;
using CatApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using CatApi.Configuration;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace CatApi.Services
{
    public class CatService : ICatService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly IValidator<CatEntity> _catValidator;
        private readonly IValidator<TagEntity> _tagValidator;
        private readonly ILogger<CatService> _logger;

        public CatService(AppDbContext context, HttpClient httpClient, IOptions<CatApiSettings> catApiOptions,
             IValidator<CatEntity> catValidator, IValidator<TagEntity> tagValidator, ILogger<CatService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            var settings = catApiOptions.Value;
            _catValidator = catValidator;
            _tagValidator = tagValidator;
            _logger = logger;

            _apiUrl = $"{settings.BaseUrl}images/search?limit=25&has_breeds=1";
            _httpClient.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
        }

        public async Task<string> FetchCatsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching cats from API...");

                var response = await _httpClient.GetStringAsync(_apiUrl);
                var catsFromApi = JsonConvert.DeserializeObject<List<CatApiResponse>>(response);

                if (catsFromApi == null || !catsFromApi.Any())
                {
                    _logger.LogWarning("No cats found in API response.");
                    return "No cats found in API response.";
                }

                foreach (var cat in catsFromApi)
                {
                    if (await _context.Cats.AnyAsync(c => c.CatId == cat.Id))
                    {
                        _logger.LogInformation("Cat {CatId} already exists in the database. Skipping...", cat.Id);
                        continue;
                    }

                    var catEntity = new CatEntity
                    {
                        CatId = cat.Id,
                        Width = cat.Width,
                        Height = cat.Height,
                        Created = DateTime.UtcNow
                    };

                    var catValidationResult = await _catValidator.ValidateAsync(catEntity);
                    if (!catValidationResult.IsValid)
                    {
                        _logger.LogWarning("Validation failed for cat {CatId}: {Errors}",
                            catEntity.CatId,
                            string.Join(", ", catValidationResult.Errors.Select(e => e.ErrorMessage)));
                        continue;
                    }

                    foreach (var breed in cat.Breeds)
                    {
                        var tags = breed.Temperament.Split(", ").Select(t => t.Trim()).Distinct();
                        foreach (var tagName in tags)
                        {
                            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                            if (tag == null)
                            {
                                tag = new TagEntity { Name = tagName, Created = DateTime.UtcNow };

                                var tagValidationResult = await _tagValidator.ValidateAsync(tag);
                                if (!tagValidationResult.IsValid)
                                {
                                    _logger.LogWarning("Validation failed for tag '{TagName}': {Errors}",
                                        tagName,
                                        string.Join(", ", tagValidationResult.Errors.Select(e => e.ErrorMessage)));
                                    continue;
                                }

                                _context.Tags.Add(tag);
                            }
                            catEntity.Tags.Add(tag);
                            await _context.SaveChangesAsync();
                        }
                    }

                    var imagePath = await SaveImageLocally(cat.Url);
                    catEntity.ImagePath = imagePath;

                    _context.Cats.Add(catEntity);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully fetched and stored new cats.");
                return "Fetched and stored new cats.";
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error occurred while calling the external Cat API.");
                return "Failed to fetch cats from the API. Please try again later.";
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while saving cat data.");
                return "Failed to save cat data. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in FetchCatsAsync.");
                return "An error occurred while processing cat data.";
            }
        }

        public async Task<CatEntity?> GetCatByIdAsync(int id)
        {
            try
            {
                return await _context.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cat by ID {CatId}.", id);
                return null;
            }
        }

        public async Task<(int totalCount, List<CatEntity> cats)> GetCatsAsync(string? tag, int page, int pageSize)
        {
            try
            {
                var query = _context.Cats.Include(c => c.Tags).AsQueryable();

                if (!string.IsNullOrEmpty(tag))
                {
                    query = query.Where(c => c.Tags.Any(t => t.Name == tag));
                }

                var totalCount = await query.CountAsync();
                var cats = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return (totalCount, cats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paginated cats.");
                return (0, new List<CatEntity>());
            }
        }

        private async Task<string> SaveImageLocally(string imageUrl)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                var fileName = Path.GetFileName(imageUrl);
                var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "CatImages");

                if (!Directory.Exists(imageFolderPath))
                {
                    Directory.CreateDirectory(imageFolderPath);
                }

                var filePath = Path.Combine(imageFolderPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                return filePath;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Failed to download image from {ImageUrl}", imageUrl);
                return "Failed to save image.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving image from {ImageUrl}", imageUrl);
                return "Failed to save image.";
            }
        }
    }
}
