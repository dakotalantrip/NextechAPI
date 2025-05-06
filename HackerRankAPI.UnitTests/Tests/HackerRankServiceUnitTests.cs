using System.Net;
using System.Text.Json;
using AutoMapper;
using HackerRank.DataTransferObjects;
using HackerRank.Models;
using HackerRank.Services;
using HackerRankAPI.TestingUtilities.Comparers;
using HackerRankAPI.TestingUtilities.Data;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;

namespace HackerRank.UnitTests.UnitTests
{
    public class HackerRankServiceUnitTests
    {
        private readonly string cacheKey = HackerRankDataFactory.cacheKey;
        private readonly string nonExistentSearchTerm = HackerRankDataFactory.nonExistentSearchTerm();
        private readonly string searchTerm = HackerRankDataFactory.searchTerm;
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
            var items = HackerRankDataFactory.GetItems();
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
                        return SetHttpResponseMessage(HttpStatusCode.OK, ids);
                    }
                    else if (request.RequestUri.AbsolutePath.Contains("item/"))
                    {
                        var id = int.Parse(request.RequestUri.AbsolutePath.Replace(".json", string.Empty).Split('/').Last());
                        var story = items.FirstOrDefault(s => s.ID == id);
                        return SetHttpResponseMessage(HttpStatusCode.OK, story);
                    }

                    return SetHttpResponseMessage(HttpStatusCode.NotFound);
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

        #region Get

        [Fact]
        public async Task Get_Returns()
        {
            // Act
            var result = await _hackerRankService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<HackerRankItemDTO>>(result);
        }

        [Fact]
        public async Task Get_ReturnsAny()
        {
            // Act
            var result = await _hackerRankService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Get_SetsCache()
        {
            // Arrange
            var mockEntry = new Mock<ICacheEntry>();
            _mockMemoryCache
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(mockEntry.Object);

            // Act
            await _hackerRankService.Get();

            // Assert
            _mockMemoryCache.Verify(x => x.CreateEntry(It.Is<object>(key => key.Equals(cacheKey))), Times.Once);
        }

        [Fact]
        public async Task Get_ReturnsCache()
        {
            // Arrange
            const string cacheKey = "responses";
            var cachedItems = HackerRankDataFactory.GetItemDTOs();

            // Mock the cache to simulate a cache hit
            _mockMemoryCache
                .Setup(mc => mc.TryGetValue(It.Is<object>(key => key.Equals(cacheKey)), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) =>
                {
                    value = cachedItems;
                })
                .Returns(true);

            // Act
            var result = await _hackerRankService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cachedItems, result, new HackerRankItemDTOComparer());

            // Verify that no HTTP calls were made
            _mockHttpClientFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Get_Handles_HTTPError()
        {
            // Arrange
            var function = (HttpRequestMessage request, CancellationToken token) => {
                return SetHttpResponseMessage(HttpStatusCode.InternalServerError, new StringContent("Internal Server Error"));
            };
            var service = SetHackerRankService(function);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => service.Get());
            Assert.Contains("Error retrieving newest stories", exception.Message);
        }

        [Fact]
        public async Task Get_Handles_EmptyIDs()
        {
            // Arrange
            var function = (HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("newstories.json"))
                {
                    return SetHttpResponseMessage(HttpStatusCode.OK, new List<int>());
                }

                return SetHttpResponseMessage(HttpStatusCode.NotFound);
            };
            var service = SetHackerRankService(function);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => service.Get());
            Assert.Contains("No items found", exception.Message);
        }

        [Fact]
        public async Task Get_Handles_EmptyItems()
        {
            // Arrange
            var function = (HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("newstories.json"))
                {
                    return SetHttpResponseMessage(HttpStatusCode.OK, HackerRankDataFactory.GetIDs());
                }
                else if (request.RequestUri!.AbsolutePath.Contains("item/"))
                {
                    return SetHttpResponseMessage(HttpStatusCode.OK);
                }

                return SetHttpResponseMessage(HttpStatusCode.NotFound);
            };
            var service = SetHackerRankService(function);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await service.Get());
            Assert.Contains("Error retrieving newest stories", exception.Message);
        }

        [Fact]
        public async Task Get_Handles_InvalidIDs()
        {
            // Arrange
            var function = (HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("newstories.json"))
                {
                    return SetHttpResponseMessage(HttpStatusCode.OK, new List<string>() { "!@#$", "%^&*", "()" });
                }

                return SetHttpResponseMessage(HttpStatusCode.NotFound);
            };
            var service = SetHackerRankService(function);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await service.Get());
            Assert.Contains("Error retrieving newest stories", exception.Message);
        }

        [Fact]
        public async Task Get_Handles_InvalidItems()
        {
            // Arrange
            var function = (HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("newstories.json"))
                {
                    return SetHttpResponseMessage(HttpStatusCode.OK, HackerRankDataFactory.GetIDs());
                }
                else if (request.RequestUri!.AbsolutePath.Contains("item/"))
                {
                    return SetHttpResponseMessage(HttpStatusCode.OK, new List<string>() { "abcd", "efgh" });
                }

                return SetHttpResponseMessage(HttpStatusCode.NotFound);
            };
            var service = SetHackerRankService(function);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await service.Get());
            Assert.Contains("Error retrieving newest stories", exception.Message);
        }

        #endregion

        #region GetPaginated

        [Fact]
        public async Task GetPaginated_Returns()
        {
            var searchResults = await _hackerRankService.GetPaginated("", 1, 10);

            Assert.NotNull(searchResults);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(searchResults);
        }

        [Fact]
        public async Task GetPaginated_ReturnsAny()
        {
            var item = HackerRankDataFactory.GetItemDTOs().FirstOrDefault();
            Assert.NotNull(item);

            var searchResults = await _hackerRankService.GetPaginated(item.Title, 1, 10);

            Assert.NotNull(searchResults);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(searchResults);
            Assert.NotEmpty(searchResults.Items);
        }

        [Fact]
        public async Task GetPaginated_EmptySearch_ReturnsFirstPage()
        {
            // Arrange
            var items = HackerRankDataFactory.GetItemDTOs();
            _mockMemoryCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) =>
                {
                    value = items;
                })
                .Returns(true);

            // Act
            var pageSize = 10;
            var result = await _hackerRankService.GetPaginated("", 1, pageSize);
            var paginatedItems = items.Take(pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(paginatedItems, result.Items, new HackerRankItemDTOComparer());
        }

        [Fact]
        public async Task GetPaginated_ReturnsCorrectPagination()
        {
            // Arrange
            var items = HackerRankDataFactory.GetItemDTOs();
            _mockMemoryCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) =>
                {
                    value = items;
                })
                .Returns(true);

            const int page = 2;
            const int pageSize = 10;

            // Act
            var result = await _hackerRankService.GetPaginated("", page, pageSize);
            var paginatedItems = items.Skip((page - 1) * pageSize).Take(pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(paginatedItems, result.Items, new HackerRankItemDTOComparer());
        }

        [Fact]
        public async Task GetPaginated_FiltersResult()
        {
            // Arrange
            var items = HackerRankDataFactory.GetItemDTOs();
            _mockMemoryCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) => { value = items; })
                .Returns(true);

            // Act
            var result = await _hackerRankService.GetPaginated(searchTerm, 1, 10);

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

        [Fact]
        public async Task GetPaginated_NoMatch_ReturnsEmpty()
        {
            // Arrange
            var items = HackerRankDataFactory.GetItemDTOs();
            _mockMemoryCache
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) => { value = items; })
                .Returns(true);

            // Act
            var result = await _hackerRankService.GetPaginated(nonExistentSearchTerm, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetPaginated_InvalidPaginationParameters_ReturnsDefault()
        {
            // Arrange
            var items = HackerRankDataFactory.GetItemDTOs();
            _mockMemoryCache
                .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) => { value = items; })
                .Returns(true);

            // Act
            var resultWithZeroPageSize = await _hackerRankService.GetPaginated("", 1, 0);
            var resultWithNegativePage = await _hackerRankService.GetPaginated("", -1, 10);

            // Assert
            Assert.NotNull(resultWithZeroPageSize);
            Assert.NotNull(resultWithZeroPageSize.Items);
            Assert.NotEmpty(resultWithZeroPageSize.Items);
            Assert.Equal(resultWithZeroPageSize.Items?.Count(), 10);

            Assert.NotNull(resultWithNegativePage);
            Assert.NotNull(resultWithNegativePage.Items);
            Assert.NotEmpty(resultWithNegativePage.Items);
            Assert.Equal(resultWithNegativePage.Items?.Count(), 10);
        }

        [Fact]
        public async Task GetPaginated_PageNumberExceedsTotalPages_ReturnsEmpty()
        {
            // Arrange
            var items = HackerRankDataFactory.GetItemDTOs();
            _mockMemoryCache
                .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Callback((object key, out object value) => { value = items; })
                .Returns(true);

            const int page = 100; // Assume there are fewer than 100 pages
            const int pageSize = 10;

            // Act
            var result = await _hackerRankService.GetPaginated("", page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        #endregion

        #region Utility

        private HackerRankService SetHackerRankService(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> function)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(function);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new HackerRankService(httpClient, _mockMapper.Object, _mockMemoryCache.Object);

            return service;
        }

        private HttpResponseMessage SetHttpResponseMessage(HttpStatusCode httpStatusCode, object? content = null)
        {
            return new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent(JsonSerializer.Serialize(content))
            };
        }

        #endregion
    }
}