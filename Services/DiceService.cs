using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RollForge.Api.Hubs;
using RollForge.Api.Models;

namespace RollForge.Api.Services
{
    public class DiceService
    {
        private readonly SessionService _sessionService;
        private readonly IHubContext<GameHub> _hubContext;

        public DiceService(SessionService sessionService, IHubContext<GameHub> hubContext)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
        }

        public async Task<Roll> RollDice(string sessionId, string playerName, DiceTypeEnum dice)
        {
            var sides = (int)dice;
            var result = new Random().Next(1, sides + 1);

            var roll = new Roll
            {
                Player = playerName,
                Dice = dice,
                Result = result,
                Timestamp = DateTime.UtcNow
            };

            _sessionService.AddRoll(sessionId, roll);

            await _hubContext.Clients.Group(sessionId)
                .SendAsync("DiceRolled", new
                {
                    player = roll.Player,
                    dice = roll.Dice,
                    result = roll.Result,
                    timestamp = roll.Timestamp
                });

            return roll;
        }
    }
}