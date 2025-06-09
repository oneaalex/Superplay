using Shared.Models;
using Shared.Messages;

namespace GameServer.Services;

public interface IPlayerRepository
{
    PlayerState GetOrCreatePlayer(string playerId, string deviceId);
    bool TryGetPlayer(string playerId, out PlayerState? state);
    bool UpdateResource(string playerId, ResourceType type, int amount, out int newBalance);
    bool TransferResource(string fromPlayerId, string toPlayerId, ResourceType type, int value);
    bool RemovePlayer(string playerId);
}