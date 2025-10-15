using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using RollForge.Api.Models;
using RollForge.Api.Services;

namespace RollForge.Api.Hubs;

public class GameHub : Hub
{
    private readonly SessionService _sessionService;
    private readonly DiceService _diceService;

    private static readonly ConcurrentDictionary<string, (string SessionId, string PlayerName)> _connections = new();

    public GameHub(SessionService sessionService, DiceService diceService)
    {
        _sessionService = sessionService;
        _diceService = diceService;
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

        try
        {
            _sessionService.AddPlayer(sessionId, playerName);

            _connections[Context.ConnectionId] = (sessionId, playerName);

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

            await Clients.Group(sessionId).SendAsync("PlayerJoined", new
            {
                player = playerName,
                message = $"{playerName} entrou na sessão."
            });

            await Clients.Caller.SendAsync("JoinedSession", new
            {
                sessionId = sessionId,
                sessionName = session.Name,
                playerName = playerName,
                message = "Você entrou na sessão com sucesso!"
            });
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message });
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", new { message = "Erro ao entrar na sessão." });
        }
    }

    public async Task RollDice(string sessionId, string playerName, DiceTypeEnum dice)
    {
        try
        {
            var roll = await _diceService.RollDice(sessionId, playerName, dice);

            await Clients.Caller.SendAsync("RollSuccess", new
            {
                message = "Dado rolado com sucesso!"
            });
        }
        catch (KeyNotFoundException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message });
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", new { message = "Erro ao rolar dados." });
        }
    }

    public async Task LeaveSession(string sessionId, string playerName)
    {
        try
        {
            var session = _sessionService.GetSession(sessionId);
            if (session is null)
            {
                await Clients.Caller.SendAsync("Error", new { message = "Sessão não encontrada." });
                return;
            }

            var isMaster = session.Players
                .Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase) && p.IsMaster);

            _sessionService.RemovePlayer(sessionId, playerName);

            _connections.TryRemove(Context.ConnectionId, out _);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

            if (isMaster)
            {
                await Clients.Group(sessionId).SendAsync("SessionEnded", new
                {
                    sessionId = sessionId,
                    message = $"O mestre {playerName} saiu. Sessão encerrada.",
                    reason = "master_left"
                });

                await Clients.Caller.SendAsync("LeftSession", new
                {
                    message = "Você saiu da sessão. Mesa encerrada."
                });

                return;
            }

            await Clients.Group(sessionId).SendAsync("PlayerLeft", new
            {
                player = playerName,
                message = $"{playerName} saiu da sessão."
            });

            if (!_sessionService.SessionExists(sessionId))
            {
                await Clients.Group(sessionId).SendAsync("SessionEnded", new
                {
                    sessionId = sessionId,
                    message = "Sessão encerrada (sem jogadores).",
                    reason = "no_players"
                });
            }

            await Clients.Caller.SendAsync("LeftSession", new
            {
                message = "Você saiu da sessão."
            });
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", new { message = "Erro ao sair da sessão." });
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var connection))
        {
            var (sessionId, playerName) = connection;

            try
            {
                var session = _sessionService.GetSession(sessionId);
                if (session is not null)
                {
                    var isMaster = session.Players
                        .Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase) && p.IsMaster);

                    _sessionService.RemovePlayer(sessionId, playerName);

                    if (isMaster)
                    {
                        await Clients.Group(sessionId).SendAsync("SessionEnded", new
                        {
                            sessionId = sessionId,
                            message = $"O mestre {playerName} desconectou. Sessão encerrada.",
                            reason = "master_disconnected"
                        });
                    }
                    else
                    {
                        await Clients.Group(sessionId).SendAsync("PlayerLeft", new
                        {
                            player = playerName,
                            message = $"{playerName} desconectou."
                        });

                        if (!_sessionService.SessionExists(sessionId))
                        {
                            await Clients.Group(sessionId).SendAsync("SessionEnded", new
                            {
                                sessionId = sessionId,
                                message = "Sessão encerrada (sem jogadores).",
                                reason = "no_players"
                            });
                        }
                    }
                }
            }
            catch
            {
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
