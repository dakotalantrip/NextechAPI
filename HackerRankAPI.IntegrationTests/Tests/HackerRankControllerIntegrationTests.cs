using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HackerRank.Controllers;
using HackerRank.DataTransferObjects;
using HackerRank.Models;
using HackerRank.Services;
using HackerRankAPI.TestingUtilities.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace HackerRankAPI.IntegrationTests.Tests
{
    public class HackerRankControllerIntegrationTests
    {
        private readonly HackerRankController _hackerRankController;
        private readonly string nonExistentSearchTerm = HackerRankDataFactory.nonExistentSearchTerm();
        private readonly string searchTerm = HackerRankDataFactory.searchTerm;

        public HackerRankControllerIntegrationTests() 
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

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            HackerRankService hackerRankService = new HackerRankService(
                httpClient,
                mapper,
                memoryCache
            );
            _hackerRankController = new HackerRankController(hackerRankService);
        }

        [Fact]
        public async Task GetNew_ReturnsValidResponse()
        {
            // Act
            var result = await _hackerRankController.GetNew() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.IsAssignableFrom<IEnumerable<HackerRankItemDTO>>(result.Value);
            Assert.NotEmpty(result.Value as IEnumerable<HackerRankItemDTO>);
        }

        [Fact]
        public async Task GetNewPaginated_ReturnsValidResponse()
        {
            // Act
            var result = await _hackerRankController.GetNewPaginated(searchTerm, 1, 10) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(result.Value);
            Assert.NotEmpty((result.Value as PaginatedResultDTO<HackerRankItemDTO>).Items);
        }

        [Fact]
        public async Task GetNewPaginated_NoMatch_ReturnsValidResponse()
        {
            // Act
            var result = await _hackerRankController.GetNewPaginated(nonExistentSearchTerm, 1, 10) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(result.Value);
            Assert.Empty((result.Value as PaginatedResultDTO<HackerRankItemDTO>).Items);
        }

        [Fact]
        public async Task GetNewPaginated_Handles_InvalidParameters()
        {
            // Act
            var result = await _hackerRankController.GetNewPaginated(searchTerm, -1, 0) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.IsAssignableFrom<PaginatedResultDTO<HackerRankItemDTO>>(result.Value);
            Assert.NotEmpty((result.Value as PaginatedResultDTO<HackerRankItemDTO>).Items);
        }
    }
}
