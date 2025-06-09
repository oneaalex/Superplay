using System.Collections.Concurrent;
using Shared.Models;
using GameServer.Services;

namespace GameServer;

public class GameServerContext
{
    public ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> ConnectedPlayers { get; } = new();
    public PlayerService PlayerService { get; } = new();
}