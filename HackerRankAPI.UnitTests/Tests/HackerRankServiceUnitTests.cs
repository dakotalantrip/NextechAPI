using System.Net;
using System.Text.Json;
using AutoMapper;
using HackerRank.DataTransferObjects;
using HackerRank.Models;
using HackerRank.Services;
using HackerRank.UnitTests.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Services;
using Moq;
using Moq.Protected;

namespace HackerRank.UnitTests.UnitTests
{
    public class HackerRankServiceUnitTests
    {
        private readonly IHackerRankService _hackerRankService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        public HackerRankServiceUnitTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockMapper = new Mock<IMapper>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            // Arrange
            var items = DataFactory.GetItems();
            var ids = items.Select(x => x.ID);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    if (request.RequestUri!.AbsolutePath.Contains("newstories.json"))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonSerializer.Serialize(ids))
                        };
                    }
                    else if (request.RequestUri.AbsolutePath.Contains("item/"))
                    {
                        var id = int.Parse(request.RequestUri.AbsolutePath.Replace(".json", string.Empty).Split('/').Last());
                        var story = items.FirstOrDefault(s => s.ID == id);
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonSerializer.Serialize(story))
                        };
                    }

                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            _mockMapper.Setup(m => m.Map<HackerRankItemDTO>(It.IsAny<HackerRankItem>()))
                .Returns((HackerRankItem item) => new HackerRankItemDTO
                {
                    ID = item.ID,
                    Title = item.Title,
                    Url = item.Url,
                    Author = item.By,
                    Time = DateTimeOffset.FromUnixTimeSeconds(item.Time)
                });

            // Simulate a cache miss
            _mockMemoryCache
                .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false);

            // Simulate adding an entry to the cache
            _mockMemoryCache
                .Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            _hackerRankService = new HackerRankService(
                httpClient,
                _mockMapper.Object,
                _mockMemoryCache.Object);
        }

        #region GetNew

        [Fact]
        public async Task GetNew_Returns()
        {
            var result = await _hackerRankService.GetNew();

            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<HackerRankItemDTO>>(result);
        }

        [Fact]
        public async Task GetNew_ReturnsAny()
        {
            var result = await _hackerRankService.GetNew();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetNewWithUrl_Returns()
        {
            var result = await _hackerRankService.GetNewWithUrl();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetNewWithUrl_ReturnsValid()
        {
            var result = await _hackerRankService.GetNewWithUrl();

            Assert.NotNull(result);
            Assert.Equal(result.Where(x => !string.IsNullOrEmpty(x.Url)).Count(), DataFactory.GetItemDTOs().Where(x => !string.IsNullOrEmpty(x.Url)).Count());
        }

        #endregion

        #region Search

        [Fact]
        public async Task Search_Returns()
        {
            var searchResults = await _hackerRankService.Search("", 1, 10);

            Assert.NotNull(searchResults);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(searchResults);
        }

        [Fact]
        public async Task Search_ReturnsAny()
        {
            var item = DataFactory.GetItemDTOs().FirstOrDefault();
            Assert.NotNull(item);

            var searchResults = await _hackerRankService.Search(item.Title, 1, 10);

            Assert.NotNull(searchResults);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(searchResults);
            Assert.NotEmpty(searchResults.Items);
        }

        #endregion
    }
}