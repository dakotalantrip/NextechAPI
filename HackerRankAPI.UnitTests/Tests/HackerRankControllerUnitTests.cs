using System.Security.Cryptography;
using System.Text;
using HackerRank.Controllers;
using HackerRank.DataTransferObjects;
using HackerRank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using HackerRankAPI.TestingUtilities.Data;

namespace HackerRank.UnitTests.UnitTests
{
    public class HackerRankControllerUnitTests
    {
        private readonly HackerRankController _hackerRankController;
        private readonly string nonExistentSearchTerm = HackerRankDataFactory.nonExistentSearchTerm();
        private readonly Mock<IHackerRankService> _mockService;

        public HackerRankControllerUnitTests()
        {
            _mockService = new Mock<IHackerRankService>();
            _hackerRankController = new HackerRankController(_mockService.Object);
        }

        #region GetNew

        [Fact]
        public async Task GetNew_ReturnsOk()
        {
            // Arrange
            var mockData = HackerRankDataFactory.GetItemDTOs();
            _mockService.Setup(service => service.Get()).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.GetNew() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task GetNew_ReturnsNotEmpty()
        {
            // Arrange
            var mockData = HackerRankDataFactory.GetItemDTOs();
            _mockService.Setup(service => service.Get()).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.GetNew() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Value as IEnumerable<HackerRankItemDTO>);
        }

        [Fact]
        public async Task GetNew_ReturnsEmpty()
        {
            // Arrange
            _mockService.Setup(service => service.Get()).ReturnsAsync(new List<HackerRankItemDTO>());

            // Act
            var result = await _hackerRankController.GetNew() as OkObjectResult;

            // Assert
            Assert.NotNull(result?.Value);
            Assert.Empty(result.Value as IEnumerable<HackerRankItemDTO>);
        }

        #endregion

        #region GetNewPaginated

        [Fact]
        public async Task GetNewPaginated_ReturnsOk()
        {
            // Arrange
            var mockData = HackerRankDataFactory.GetPaginatedResult();
            _mockService.Setup(service => service.GetPaginated("test", 1, 10)).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.GetNewPaginated("test", 1, 10) as OkObjectResult;

            // Assert
            Assert.IsAssignableFrom<OkObjectResult>(result);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Fact]
        public async Task GetNewPaginated_ReturnsNotEmpty()
        {
            // Arrange
            var mockData = HackerRankDataFactory.GetPaginatedResult();
            var testString = string.Join(",", mockData.Items.Select(x => x.Title));
            _mockService.Setup(service => service.GetPaginated(testString, 1, 10)).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.GetNewPaginated(testString, 1, 10) as OkObjectResult;
            var value = result?.Value as PaginatedResultDTO<HackerRankItemDTO>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(value);
            Assert.NotEmpty(value.Items);
        }

        [Fact]
        public async Task GetNewPaginated_ReturnsEmpty()
        {
            // Arrange
            var mockData = HackerRankDataFactory.GetPaginatedResult();
            _mockService.Setup(service => service.GetPaginated(nonExistentSearchTerm, 1, 10)).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.GetNewPaginated(nonExistentSearchTerm, 1, 10) as OkObjectResult;
            var value = result?.Value as PaginatedResultDTO<HackerRankItemDTO>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(value);
            Assert.NotEmpty(value.Items);
        }

        #endregion
    }
}
