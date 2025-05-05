using AutoMapper;
using System.Net.Http;
using HackerRank.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using HackerRank.Models;
using HackerRank.DataTransferObjects;

namespace HackerRank.IntegrationTests.Tests
{
    public class HackerRankServiceIntegrationTests
    {
        private readonly IHackerRankService _hackerRankService;

        public HackerRankServiceIntegrationTests()
        {
            // Set up HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
            };

            // Set up IMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<HackerRankItem, HackerRankItemDTO>()
                    .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.By))
                    .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.Time)));
            });
            var mapper = mapperConfig.CreateMapper();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            _hackerRankService = new HackerRankService(
                httpClient,
                mapper,
                memoryCache
            ); ;
        }

        [Fact]
        public async Task GetNew_ReturnsValidData()
        {
            // Act
            var result = await _hackerRankService.GetNew();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, item =>
            {
                Assert.NotNull(item.Title);
                Assert.NotNull(item.Url);
            });
        }

        [Fact]
        public async Task Search_ReturnsResults()
        {
            // Arrange
            var searchTerm = "test";

            // Act
            var result = await _hackerRankService.Search(searchTerm, 1 , 10);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.True(
                    item.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    item.Url.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    item.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase),
                    $"The search term '{searchTerm}' was not found in Title, Url, or Author of item with ID {item.ID}."
                );
            });
        }
    }
}
