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
        if (string.IsNullOrWhiteSpace(playerName))
        {
            await Clients.Caller.SendAsync("Error", new { message = "Nome do jogador não pode ser vazio." });
            return;
        }

        var session = _sessionService.GetSession(sessionId);
        if (session is null)
        {
            await Clients.Caller.SendAsync("Error", new { message = "Sessão não encontrada." });
            return;
        }

        bool alreadyInSession = session.Players.Any(p =>
            p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        if (alreadyInSession)
        {
            await Clients.Group(sessionId).SendAsync("PlayerReconnected", new
            {
                player = playerName,
                message = $"{playerName} reconectou à sessão."
            });
        }
        else
        {
            _sessionService.AddPlayer(sessionId, playerName); // 🔥 só se ainda não existir
            await Clients.Group(sessionId).SendAsync("PlayerJoined", new
            {
                player = playerName,
                message = $"{playerName} entrou na sessão."
            });
        }
    }
}
