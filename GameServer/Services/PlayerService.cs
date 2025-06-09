using System.Collections.Concurrent;
using Shared.Models;
using Shared.Messages;

namespace GameServer.Services;

public class PlayerService : IPlayerRepository
{
    private readonly ConcurrentDictionary<string, PlayerState> _players = new();

    public PlayerState GetOrCreatePlayer(string playerId, string deviceId)
    {
        return _players.GetOrAdd(playerId, id => new PlayerState
        {
            PlayerId = playerId,
            DeviceId = deviceId
        });
    }

    public bool TryGetPlayer(string playerId, out PlayerState? state) => _players.TryGetValue("P_" + playerId, out state);

    public bool UpdateResource(string playerId, ResourceType type, int amount, out int newBalance)
    {
        newBalance = 0;
        if (!_players.TryGetValue("P_" + playerId, out var player))
            return false;

        lock (player)
        {
            if (!player.Resources.ContainsKey(type))
                player.Resources[type] = 0;
            player.Resources[type] += amount;
            newBalance = player.Resources[type];
        }
        return true;
    }

    public bool TransferResource(string fromPlayerId, string toPlayerId, ResourceType type, int value)
    {
        if (!_players.TryGetValue("P_" + fromPlayerId, out var from) ||
            !_players.TryGetValue("P_" + toPlayerId, out var to))
            return false;

        lock (from)
            lock (to)
            {
                if (!from.Resources.ContainsKey(type) || from.Resources[type] < value)
                    return false;

                from.Resources[type] -= value;
                if (!to.Resources.ContainsKey(type))
                    to.Resources[type] = 0;
                to.Resources[type] += value;
            }
        return true;
    }

    public bool RemovePlayer(string playerId)
        => _players.TryRemove(playerId, out _);
}