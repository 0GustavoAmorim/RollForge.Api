using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RollForge.Api.Models
{
    public class Player
    {
        public string Name { get; set; } = string.Empty;
        public bool IsMaster { get; set; } = false;
    }
}