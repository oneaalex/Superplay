using System.Collections.Concurrent;
using Shared.Models;
using Shared.Messages;

namespace GameServer.Services;

public class PlayerService : IPlayerRepository
{
    private readonly ConcurrentDictionary<string, PlayerState> _players = new();

    public PlayerState GetOrCreatePlayer(string playerId, string deviceId)
    {
        // Always store with "P_" prefix for consistency
        var key = "P_" + playerId;
        return _players.GetOrAdd(key, id => new PlayerState
        {
            PlayerId = key,
            DeviceId = deviceId
        });
    }

    public bool TryGetPlayer(string playerId, out PlayerState? state)
    {
        // Always look up with "P_" prefix
        return _players.TryGetValue("P_" + playerId, out state);
    }

    public bool UpdateResource(string playerId, ResourceType type, int amount, out int newBalance)
    {
        newBalance = 0;
        if (!TryGetPlayer(playerId, out var player))
            return false;

        lock (player!)
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
        if (!TryGetPlayer(fromPlayerId, out var from) ||
            !TryGetPlayer(toPlayerId, out var to))
            return false;

        lock (from!)
            lock (to!)
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
        => _players.TryRemove("P_" + playerId, out _);
}