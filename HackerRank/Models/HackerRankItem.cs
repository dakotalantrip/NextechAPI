using HackerRank.Enums;

namespace HackerRank.Models
{
    public class HackerRankItem
    {
        public int ID { get; set; }
        public bool Deleted { get; set; }
        public string Type { get; set; }
        public string By { get; set; } = String.Empty;
        public long Time { get; set; }
        public string? Text { get; set; } = string.Empty;
        public bool Dead { get; set; }
        public int Parent { get; set; }
        public int Poll { get; set; }
        public List<int> Kids { get; set; } = new List<int>();
        public string Url { get; set; } = string.Empty; 
        public int Score { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<int> Parts { get; set; } = new List<int>();
        public int Descendants { get; set; }
    }
}
