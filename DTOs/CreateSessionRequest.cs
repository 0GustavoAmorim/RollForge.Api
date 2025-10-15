using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RollForge.Api.DTOs
{
    public class CreateSessionRequest
    {
        public string SessionName { get; set; } = string.Empty;
        public string MasterName { get; set; } = string.Empty;
    }
}