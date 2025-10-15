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

            if (string.IsNullOrWhiteSpace(playerName))
                throw new ArgumentException("Nome do jogador não pode ser vazio.", nameof(playerName));

            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("ID da sessão não pode ser vazio.", nameof(sessionId));

            var session = _sessionService.GetSession(sessionId);
            if (session is null)
                throw new KeyNotFoundException("Sessão não encontrada.");

            var player = session.Players
                       .FirstOrDefault(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

            if (player is null)
                throw new InvalidOperationException($"O jogador '{playerName}' não faz parte desta sessão.");

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
                    timestamp = roll.Timestamp,
                    message = $"{roll.Player} rolou um {roll.Dice} e tirou {roll.Result}."
                });

            return roll;
        }
    }
}