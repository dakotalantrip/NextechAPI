using AutoMapper;
using HackerRank.DataTransferObjects;
using HackerRank.Models;
using HackerRank.Services;
using HackerRankAPI.TestingUtilities.Data;
using Microsoft.Extensions.Caching.Memory;

namespace HackerRank.IntegrationTests.Tests
{
    public class HackerRankServiceIntegrationTests
    {
        private readonly string cacheKey = HackerRankDataFactory.cacheKey;
        private readonly string searchTerm = HackerRankDataFactory.searchTerm;
        private readonly IHackerRankService _hackerRankService;
        private readonly IMemoryCache _memoryCache;

        public HackerRankServiceIntegrationTests()
        {
            // Set up HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(HackerRankDataFactory.hackerRankUrl)
            };

            // Set up IMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<HackerRankItem, HackerRankItemDTO>()
                    .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.By))
                    .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.Time)));
            });
            var mapper = mapperConfig.CreateMapper();

            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _hackerRankService = new HackerRankService(
                httpClient,
                mapper,
                _memoryCache
            ); ;
        }

        [Fact]
        public async Task Get_ReturnsValidData()
        {
            // Act
            var result = await _hackerRankService.Get();

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
        public async Task Get_CachesResponse()
        {
            // Act
            var result = await _hackerRankService.Get();
            _memoryCache.TryGetValue(cacheKey, out IEnumerable<HackerRankItemDTO>? memoryCacheResult);
            var projectedResult = result.Select(x => new { x.ID, x.Title, x.Url, x.Author, x.Time });
            var projectMemoryCacheResult = memoryCacheResult?.Select(x => new { x.ID, x.Title, x.Url, x.Author, x.Time });

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(memoryCacheResult);
            Assert.Equal(projectedResult, projectMemoryCacheResult);
        }

        [Fact]
        public async Task GetPaginated_ValidData()
        {
            // Act
            var result = await _hackerRankService.GetPaginated(searchTerm, 1 , 10);

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
