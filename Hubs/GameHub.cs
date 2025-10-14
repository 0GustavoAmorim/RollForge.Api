using Microsoft.AspNetCore.SignalR;
using RollForge.Api.Models;
using RollForge.Api.Services;

namespace RollForge.Api.Hubs;

public class GameHub : Hub
{
    private readonly SessionService _sessionService;

    public GameHub(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task JoinSession(string sessionId, string playerName)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
        {
            await Clients.Caller.SendAsync("Error", "Sessão não encontrada.");
            return;
        }

        try
        {
            _sessionService.AddPlayer(sessionId, playerName);
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Group(sessionId).SendAsync("PlayerJoined", "Player", playerName, "entrou na sessão.");
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
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

        await Clients.Group(sessionId)
            .SendAsync("DiceRolled",  new
            {
                player = roll.Player,
                dice = roll.Dice,
                result = roll.Result,
                timestamp = roll.Timestamp
            });

        return roll;
    }
}