namespace HackerRank.DataTransferObjects
{
    public class HackerRankItemDTO
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty; 
        public string Author { get; set; } = string.Empty;
        public DateTimeOffset Time { get; set; }
    }
}
