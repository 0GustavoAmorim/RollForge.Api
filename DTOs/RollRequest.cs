using RollForge.Api.Models;

namespace RollForge.Api.DTOs
{
    public class RollRequest
    {
        public string Player { get; set; } = string.Empty;
        public DiceTypeEnum Dice { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}