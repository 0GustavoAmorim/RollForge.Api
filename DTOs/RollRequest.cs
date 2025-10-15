using RollForge.Api.Models;

namespace RollForge.Api.DTOs
{
    public class RollRequest
    {
        public string Name { get; set; } = string.Empty;
        public DiceTypeEnum Dice { get; set; }
    }
}