using HackerRank.DataTransferObjects;

namespace HackerRank.Services
{
    public interface IHackerRankService
    {
        Task<IEnumerable<HackerRankItemDTO>> Get();
        Task<PaginatedResultDTO<HackerRankItemDTO>> GetPaginated(string? searchTerm, int page, int pageSize);
    }
}
