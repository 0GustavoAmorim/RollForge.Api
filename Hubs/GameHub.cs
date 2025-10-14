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
            await Clients.Caller.SendAsync("Error", new
            {
                message = "Sessão não encontrada."
            });
            return;
        }

        try
        {
            _sessionService.AddPlayer(sessionId, playerName);
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Group(sessionId).SendAsync("PlayerJoined", new
            {
                player = playerName,
                message = $"{playerName} entrou na sessão.",
            });
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", new
            {
                message = ex.Message
            });
        }
    }
}