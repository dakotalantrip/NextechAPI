namespace HackerRank.DataTransferObjects
{
    public record PaginatedResultDTO<T>(IReadOnlyList<T> Items, int TotalPages, int TotalItems);
}
