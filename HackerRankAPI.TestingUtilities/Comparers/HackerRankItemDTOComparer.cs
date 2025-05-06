using HackerRank.DataTransferObjects;

namespace HackerRankAPI.TestingUtilities.Comparers
{
    public class HackerRankItemDTOComparer : IEqualityComparer<HackerRankItemDTO>
    {
        public bool Equals(HackerRankItemDTO? x, HackerRankItemDTO? y)
        {
            if (x == null || y == null) return false;

            return x.ID == y.ID &&
                   x.Title == y.Title &&
                   x.Url == y.Url &&
                   x.Author == y.Author &&
                   x.Time == y.Time;
        }

        public int GetHashCode(HackerRankItemDTO obj)
        {
            return HashCode.Combine(obj.ID, obj.Title, obj.Url, obj.Author, obj.Time);
        }
    }
}
