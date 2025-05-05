using System.Text.Json.Serialization;

namespace HackerRank.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ItemType
    {
        job,
        story,
        comment,
        poll,
        pollopt
    }
}
