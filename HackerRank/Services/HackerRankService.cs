using System.Text.Json;
using AutoMapper;
using HackerRank.DataTransferObjects;
using HackerRank.Extensions;
using HackerRank.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerRank.Services
{
    public class HackerRankService : IHackerRankService
    {
        private readonly int cacheTTL = 10;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly string url = "https://hacker-news.firebaseio.com/v0/";

        public HackerRankService(HttpClient httpClient, IMapper mapper, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<HackerRankItemDTO>> Get()
        {
            const string cacheKey = "responses";
            var cachedValue = GetCachedValue<IEnumerable<HackerRankItemDTO>>(cacheKey);
            if (cachedValue != null)
                return cachedValue;

            try 
            {
                var ids = await GetNewStoryIDs();
                var items = await GetItems(ids);
                if (items != null && items.Any())
                {
                    var dataTransferObjects = items.Select(x => _mapper.Map<HackerRankItemDTO>(x));
                    _memoryCache.Set(cacheKey, dataTransferObjects, TimeSpan.FromMinutes(cacheTTL));
                    return dataTransferObjects;
                }
                else
                {
                    throw new Exception("No items found");
                }
            } 
            catch(Exception ex) 
            {
                throw new Exception($"Error retrieving newest stories. Error: {ex.Message}");
            }
        }

        public async Task<PaginatedResultDTO<HackerRankItemDTO>> GetPaginated(string? searchTerm, int page, int pageSize)
        {
            var results = await Get();
            searchTerm = searchTerm?.ToLower().Trim() ?? "";

            var filteredResults = string.IsNullOrEmpty(searchTerm) ? results : results
                .Where(x => 
                    x.Url.ToLower().Contains(searchTerm) || 
                    x.Title.ToLower().Contains(searchTerm) ||
                    x.Author.ToLower().Contains(searchTerm));

            var paginatedResult = filteredResults.ToPaginatedResult(page, pageSize);
            return paginatedResult;
        }

        #region Utility

        private async Task<IEnumerable<int>> GetNewStoryIDs()
        {
            const string cacheKey = "ids";
            var cachedValue = GetCachedValue<IEnumerable<int>>(cacheKey);
            if (cachedValue != null)
                return cachedValue;

            try
            {
                var response = await _httpClient.GetAsync($"{url}newstories.json?print=pretty");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var storyIds = JsonSerializer.Deserialize<List<int>>(responseBody);

                    if (storyIds != null)
                    {
                        _memoryCache.Set<IEnumerable<int>>(cacheKey, storyIds, TimeSpan.FromMinutes(cacheTTL));
                        return storyIds;
                    }
                    else
                    {
                        throw new Exception($"Unable to deserialize story ids");
                    }
                }
                else
                {
                    throw new Exception($"Unable to get new story ids");
                }
            }

            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new story ids from HackerRank. Error: {ex.Message}");
            }
        }

        private async Task<IEnumerable<HackerRankItem?>> GetItems(IEnumerable<int> ids)
        {
            const string cacheKey = "stories";
            var cachedValue = GetCachedValue<IEnumerable<HackerRankItem?>>(cacheKey);
            if (cachedValue != null)
                return cachedValue;

            try
            {
                var semaphore = new SemaphoreSlim(10);
                var tasks = ids.Select(async id =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var storyResponse = await _httpClient.GetAsync($"{url}item/{id}.json?print=pretty");
                        if (storyResponse.IsSuccessStatusCode)
                        {
                            var storyBody = await storyResponse.Content.ReadAsStringAsync();
                            try
                            {
                                var hackerRankResponse = JsonSerializer.Deserialize<HackerRankItem>(storyBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                return hackerRankResponse;
                            }

                            catch (Exception ex)
                            {
                                throw new Exception($"Error deserializing data. Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var stories = (await Task.WhenAll(tasks)).Where(x => x != null).Where(x => x != null && !string.IsNullOrEmpty(x.Url));
                if (stories != null)
                {
                    _memoryCache.Set(cacheKey, stories, TimeSpan.FromMinutes(cacheTTL));
                    return stories;
                }
                else
                {
                    throw new Exception($"Unable to deserialize stories");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving story data from HackerRank. Error {ex.Message}");
            }
        }

        private T GetCachedValue<T>(string cacheKey)
        {
            if (_memoryCache.TryGetValue(cacheKey, out T? cachedValue))
            {
                if (cachedValue != null)
                    return cachedValue;
            }

            return default!;
        }

        #endregion
    }
}
