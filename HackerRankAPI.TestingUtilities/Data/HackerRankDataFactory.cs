using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HackerRank.DataTransferObjects;
using HackerRank.Models;

namespace HackerRankAPI.TestingUtilities.Data
{
    public static class HackerRankDataFactory
    {
        public static string cacheKey = "responses";
        public static string hackerRankUrl = "https://hacker-news.firebaseio.com/v0/";
        public static string searchTerm = GetItems().FirstOrDefault()?.Title ?? "test";

        public static string nonExistentSearchTerm() 
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(searchTerm));
            var uniqueString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return uniqueString;
        }

        public static IEnumerable<int> GetIDs()
        {
            return GetItems().Select(x => x.ID);
        }

        public static IEnumerable<HackerRankItem> GetItems()
        {
            return GetItemsFromJson();
        }

        public static IEnumerable<HackerRankItemDTO> GetItemDTOs()
        {
            return GetItemsFromJson().Select(x => new HackerRankItemDTO
            {
                ID = x.ID,
                Title = x.Title,
                Url = x.Url,
                Author = x.By ?? string.Empty,
                Time = DateTimeOffset.FromUnixTimeSeconds(x.Time).ToLocalTime()
            });
        }

        private static IEnumerable<HackerRankItem> GetItemsFromJson()
        {
            var json = File.ReadAllText("./Data/HackerRankItems.json");
            var items = JsonSerializer.Deserialize<List<HackerRankItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true } );
            return items;
        }

        public static PaginatedResultDTO<HackerRankItemDTO> GetPaginatedResult()
        {
            var hackerRankItems = GetItemDTOs();

            return new PaginatedResultDTO<HackerRankItemDTO>(hackerRankItems.ToList(), 1, hackerRankItems.Count());
        }
    }
}
