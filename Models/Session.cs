using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RollForge.Api.Models
{
    public class Session
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Roll> Rolls { get; set; } = new List<Roll>();
    }
}