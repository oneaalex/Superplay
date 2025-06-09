using System.Collections.Concurrent;
using Shared.Models;
using Shared.Messages;

namespace GameServer.Services;

public class PlayerService : IPlayerRepository
{
    private readonly ConcurrentDictionary<string, PlayerState> _players = new();
    private static string MakeKey(string playerId) => $"P_{playerId}";

    public PlayerState GetOrCreatePlayer(string playerId, string deviceId)
    {
        var key = MakeKey(playerId);
        return _players.GetOrAdd(key, _ => new PlayerState
        {
            PlayerId = key,
            DeviceId = deviceId
        });
    }

    public bool TryGetPlayer(string playerId, out PlayerState? state)
        => _players.TryGetValue(MakeKey(playerId), out state);

    public bool TryGetPlayerById(string playerId, out PlayerState? state)
        => _players.TryGetValue(playerId, out state);

    public bool UpdateResource(string playerId, ResourceType type, int amount, out int newBalance)
    {
        newBalance = 0;
        if (!TryGetPlayerById(playerId, out var player) || player is null)
            return false;
        lock (player)
        {
            player.Resources[type] = player.Resources.GetValueOrDefault(type, 0) + amount;
            newBalance = player.Resources[type];
        }
        return true;
    }

    public bool TransferResource(string fromPlayerId, string toPlayerId, ResourceType type, int value)
    {
        if (!TryGetPlayerById(fromPlayerId, out var from) || from is null ||
            !TryGetPlayerById(toPlayerId, out var to) || to is null)
            return false;
        lock (from)
            lock (to)
            {
                if (from.Resources.GetValueOrDefault(type, 0) < value)
                    return false;
                from.Resources[type] -= value;
                to.Resources[type] = to.Resources.GetValueOrDefault(type, 0) + value;
            }
        return true;
    }

    public bool RemovePlayer(string playerId)
        => _players.TryRemove(MakeKey(playerId), out _);
}