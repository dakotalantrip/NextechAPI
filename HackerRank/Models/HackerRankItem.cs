using HackerRank.Enums;

namespace HackerRank.Models
{
    public record HackerRankItem(
        int ID,
        bool Deleted,
        ItemType Type,
        string? By,
        long Time,
        string? Text,
        bool Dead,
        int Parent,
        int Poll,
        List<int> Kids,
        string Url,
        int Score,
        List<int> Parts,
        int Descendants,
        string Title = ""
    );
}
