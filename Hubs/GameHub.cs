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
            throw new HubException("Sessão não encontrada.");
        }

        try
        {
            _sessionService.AddPlayer(sessionId, playerName);
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Group(sessionId).SendAsync("Player", playerName, "entrou na sessão.");
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task RollDice(string sessionId, string playerName, string diceType)
    {
        var sides = GetDiceSides(diceType);
        var result = new Random().Next(1, sides + 1);

        var roll = new Roll
        {
            Player = playerName,
            Dice = diceType,
            Result = result,
            Timestamp = DateTime.UtcNow
        };

        _sessionService.AddRoll(sessionId, roll);
        await Clients.Group(sessionId).SendAsync(playerName, ": rolou", diceType, "e tirou = ", result);
    }

    private static int GetDiceSides(string diceType) => diceType.ToLower() switch
    {
        "d4" => 4,
        "d6" => 6,
        "d8" => 8,
        "d10" => 10,
        "d12" => 12,
        "d20" => 20,
        _ => 20
    };
}