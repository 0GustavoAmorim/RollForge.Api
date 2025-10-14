using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RollForge.Api.Models;

namespace RollForge.Api.Services
{
    public class SessionService
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = new();

        public Session CreateSession(string name)
        {
            var session = new Session { Name = name };
            _sessions[session.Id] = session;
            return session;
        }

        public Session? GetSession(string id)
        {
            _sessions.TryGetValue(id, out var session);
            return session;
        }

        public void AddPlayer(string sessionId, string playerName)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (session.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("Player com o mesmo nome já existe na sessão.");
                }

                if (session.Players.Count >= 10)
                {
                    throw new InvalidOperationException("Número máximo de jogadores atingido.");
                }

                session.Players.Add(new Player { Name = playerName });
                return;
            }
            throw new KeyNotFoundException("Sessão não encontrada.");
        }

        public void AddRoll(string sessionId, Roll roll)
        {
            if(_sessions.TryGetValue(sessionId, out var session))
            {
                session.Rolls.Add(roll);
            }
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }
}