using CatApi.Data;
using CatApi.Models;
using CatApi.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net;
using CatApi.Configuration;
using Moq.Protected;

namespace CatApiUnitTests
{
    [TestClass]
    public class CatServiceTests
    {
        private AppDbContext _dbContext;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private Mock<IValidator<CatEntity>> _mockCatValidator;
        private Mock<IValidator<TagEntity>> _mockTagValidator;
        private Mock<ILogger<CatService>> _mockLogger;
        private CatService _catService;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockCatValidator = new Mock<IValidator<CatEntity>>();
            _mockTagValidator = new Mock<IValidator<TagEntity>>();
            _mockLogger = new Mock<ILogger<CatService>>();

            var mockOptions = new Mock<IOptions<CatApiSettings>>();
            mockOptions.Setup(opt => opt.Value).Returns(new CatApiSettings
            {
                BaseUrl = "https://api.thecatapi.com/v1/",
                ApiKey = "test-api-key"
            });

            _catService = new CatService(
                _dbContext,
                _httpClient,
                mockOptions.Object,
                _mockCatValidator.Object,
                _mockTagValidator.Object,
                _mockLogger.Object
            );
        }


        [TestMethod]
        public async Task FetchCatsAsync_ShouldReturnMessage_WhenApiReturnsNoCats()
        {
            // Arrange
            var apiResponse = "[]";
            _mockHttpMessageHandler
                 .Protected()
                 .Setup<Task<HttpResponseMessage>>(
                     "SendAsync",
                     ItExpr.IsAny<HttpRequestMessage>(),
                     ItExpr.IsAny<CancellationToken>()
                 )
                 .ReturnsAsync(new HttpResponseMessage
                 {
                     StatusCode = HttpStatusCode.OK,
                     Content = new StringContent(apiResponse)
                 });

            // Act
            var result = await _catService.FetchCatsAsync();

            // Assert
            Assert.AreEqual("No cats found in API response.", result);
        }

        [TestMethod]
        public async Task FetchCatsAsync_ShouldHandleHttpRequestException()
        {
            // Arrange
            _mockHttpMessageHandler
                 .Protected()
                 .Setup<Task<HttpResponseMessage>>(
                     "SendAsync",
                     ItExpr.IsAny<HttpRequestMessage>(),
                     ItExpr.IsAny<CancellationToken>()
                 )
                 .Throws(new HttpRequestException("API error"));

            // Act
            var result = await _catService.FetchCatsAsync();

            // Assert
            Assert.AreEqual("Failed to fetch cats from the API. Please try again later.", result);
        }

        [TestMethod]
        public async Task FetchCatsAsync_ShouldValidateCatEntity()
        {
            // Arrange
            var apiResponse = JsonConvert.SerializeObject(new List<CatApiResponse>
            {
                new CatApiResponse { Id = "123", Url = "http://example.com/cat.jpg", Width = 200, Height = 300 }
            });

            _mockHttpMessageHandler
                 .Protected()
                 .Setup<Task<HttpResponseMessage>>(
                     "SendAsync",
                     ItExpr.IsAny<HttpRequestMessage>(),
                     ItExpr.IsAny<CancellationToken>()
                 )
                 .ReturnsAsync(new HttpResponseMessage
                 {
                     StatusCode = HttpStatusCode.OK,
                     Content = new StringContent(apiResponse)
                 });

            _mockCatValidator.Setup(v => v.ValidateAsync(It.IsAny<CatEntity>(), default))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("Width", "Width must be greater than 0.")
                }));

            // Act
            var result = await _catService.FetchCatsAsync();

            // Assert
            Assert.AreEqual("Fetched and stored new cats.", result);
        }
    }
}
