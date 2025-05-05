using HackerRank.DataTransferObjects;

namespace HackerRank.Extensions
{
    public static class IEnumerableExtensions
    {
        public static PaginatedResultDTO<T> ToPaginatedResult<T>(this IEnumerable<T> source, int page = 1, int pageSize = 10)
        {
            var itemCount = source.Count();
            var pageCount = (int)Math.Ceiling(itemCount / (double)pageSize);

            if (page < 1)
                page = 1;

            if (pageSize < 10)
                pageSize = 10;

            var results = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResultDTO<T>(results, pageCount, itemCount);
        }
    }
}
