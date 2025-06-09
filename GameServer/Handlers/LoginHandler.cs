using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Shared.Messages;
using Shared.Models;

namespace GameServer.Handlers;

public class LoginHandler : IMessageHandler
{
    public MessageType MessageType => MessageType.Login;

    public async Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId)
    {
        var request = JsonSerializer.Deserialize<LoginRequest>(message.Payload);
        if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.LoginResponse,
                Payload = JsonSerializer.Serialize(new LoginResponse { Error = "Invalid login request." })
            });
            return;
        }

        playerId = "P_" + request.DeviceId;

        // Check if player is already connected
        if (!context.ConnectedPlayers.TryAdd(playerId, socket))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.LoginResponse,
                Payload = JsonSerializer.Serialize(new LoginResponse { Error = "Already connected." })
            });
            return;
        }

        // Create or get player state
        var player = context.PlayerService.GetOrCreatePlayer(playerId, request.DeviceId);

        await Send(socket, new SocketMessage
        {
            Type = MessageType.LoginResponse,
            Payload = JsonSerializer.Serialize(new LoginResponse { PlayerId = playerId })
        });
    }

    private static async Task Send(WebSocket socket, SocketMessage msg)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}