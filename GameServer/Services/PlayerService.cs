using System.Net.WebSockets;
using Shared.Messages;

namespace GameServer.Services;

public class PlayerService
{
    // Maps deviceId to (playerId, socket)
    private readonly Dictionary<string, (string playerId, WebSocket socket)> _onlinePlayers = new();
    private int _nextId = 1;

    public LoginResponse HandleLogin(string deviceId, WebSocket socket)
    {
        if (_onlinePlayers.ContainsKey(deviceId))
        {
            return new LoginResponse { Error = "Already connected" };
        }

        var playerId = $"P{_nextId++}";
        _onlinePlayers[deviceId] = (playerId, socket);

        return new LoginResponse { PlayerId = playerId };
    }

    // Extend with more player operations
}