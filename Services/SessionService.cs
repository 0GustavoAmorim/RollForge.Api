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
            var session = new Session
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Players = new List<Player>(),
                Rolls = new List<Roll>()
            };

            _sessions.TryAdd(session.Id, session);
            return session;
        }

        public Session? GetSession(string id)
        {
            _sessions.TryGetValue(id, out var session);
            return session;
        }

        public void AddPlayer(string sessionId, string playerName)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                throw new KeyNotFoundException("Sessão não encontrada.");

            session.Players ??= new List<Player>();

            if (session.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Nome de jogador já está em uso na sessão.");

            if (session.Players.Count >= 10)
                throw new InvalidOperationException("Número máximo de jogadores atingido.");

            if (session.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                return;

            session.Players.Add(new Player { Name = playerName });
        }

        public void AddRoll(string sessionId, Roll roll)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Rolls.Add(roll);
            }
        }
        
        public void RemovePlayer(string sessionId, string playerName)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                var player = session.Players
                    .FirstOrDefault(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                    session.Players.Remove(player);

                if (!session.Players.Any())
                    RemoveSession(sessionId);
            }
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        public bool SessionExists(string sessionId)
        {
            return _sessions.ContainsKey(sessionId);
        }
    }
}