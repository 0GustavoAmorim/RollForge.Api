using System.Text.Json.Serialization;

namespace RollForge.Api.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiceTypeEnum
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12,
        D20 = 20
    }
}