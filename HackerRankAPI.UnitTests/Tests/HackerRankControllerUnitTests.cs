using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HackerRank.Controllers;
using HackerRank.DataTransferObjects;
using HackerRank.Services;
using HackerRank.UnitTests.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerRank.UnitTests.UnitTests
{
    public class HackerRankControllerUnitTests
    {
        private readonly HackerRankController _hackerRankController;
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
            var mockData = DataFactory.GetItemDTOs();
            _mockService.Setup(service => service.GetNew()).ReturnsAsync(mockData);

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
            var mockData = DataFactory.GetItemDTOs();
            _mockService.Setup(service => service.GetNew()).ReturnsAsync(mockData);

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
            _mockService.Setup(service => service.GetNew()).ReturnsAsync(new List<HackerRankItemDTO>());

            // Act
            var result = await _hackerRankController.GetNew() as OkObjectResult;

            // Assert
            Assert.NotNull(result?.Value);
            Assert.Empty(result.Value as IEnumerable<HackerRankItemDTO>);
        }

        #endregion

        #region Search

        [Fact]
        public async Task Search_ReturnsOk()
        {
            // Arrange
            var mockData = DataFactory.GetPaginatedResult();
            _mockService.Setup(service => service.Search("test", 1, 10)).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.Search("test", 1, 10) as OkObjectResult;

            // Assert
            Assert.IsAssignableFrom<OkObjectResult>(result);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Fact]
        public async Task Search_ReturnsNotEmpty()
        {
            // Arrange
            var mockData = DataFactory.GetPaginatedResult();
            var testString = string.Join(",", mockData.Items.Select(x => x.Title));
            _mockService.Setup(service => service.Search(testString, 1, 10)).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.Search(testString, 1, 10) as OkObjectResult;
            var value = result?.Value as PaginatedResultDTO<HackerRankItemDTO>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(value);
            Assert.NotEmpty(value.Items);
        }

        [Fact]
        public async Task Search_ReturnsEmpty()
        {
            // Arrange
            var mockData = DataFactory.GetPaginatedResult();
            var testString = mockData.Items.Where(x => !string.IsNullOrEmpty(x.Title)).FirstOrDefault()?.Title;

            Assert.NotNull(testString);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(testString));
            var uniqueString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            Assert.NotEqual(testString, uniqueString);

            _mockService.Setup(service => service.Search(uniqueString, 1, 10)).ReturnsAsync(mockData);

            // Act
            var result = await _hackerRankController.Search(uniqueString, 1, 10) as OkObjectResult;
            var value = result?.Value as PaginatedResultDTO<HackerRankItemDTO>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(value);
            Assert.NotEmpty(value.Items);
        }

        #endregion
    }
}
