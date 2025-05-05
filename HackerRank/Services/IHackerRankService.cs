using HackerRank.DataTransferObjects;

namespace HackerRank.Services
{
    public interface IHackerRankService
    {
        Task<IEnumerable<HackerRankItemDTO>> GetNewWithUrl();
        Task<IEnumerable<HackerRankItemDTO>> GetNew();
        Task<PaginatedResultDTO<HackerRankItemDTO>> Search(string? searchTerm, int page, int pageSize);
    }
}
