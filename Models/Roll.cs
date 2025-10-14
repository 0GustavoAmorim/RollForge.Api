using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RollForge.Api.Models
{
    public class Roll
    {
        public string Player { get; set; } = string.Empty;
        public DiceTypeEnum Dice { get; set; }
        public int Result { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}