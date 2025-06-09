using System.Collections.Concurrent;
using Shared.Models;

namespace GameServer;

public class GameServerContext
{
    public ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> ConnectedPlayers { get; } = new();
    public ConcurrentDictionary<string, PlayerState> PlayerStates { get; } = new();
}