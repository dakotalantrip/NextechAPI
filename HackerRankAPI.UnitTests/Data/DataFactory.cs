using System.Text.Json;
using HackerRank.DataTransferObjects;
using HackerRank.Models;

namespace HackerRank.UnitTests.Data
{
    public static class DataFactory
    {
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
                Author = x.By,
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
