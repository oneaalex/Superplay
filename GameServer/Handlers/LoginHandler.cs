using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Shared.Messages;
using Shared.Models;

namespace GameServer.Handlers;

public class LoginHandler : IMessageHandler
{
    public MessageType MessageType => MessageType.Login;

    public async Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId)
    {
        var request = JsonSerializer.Deserialize<LoginRequest>(message.Payload);
        if (request == null || string.IsNullOrEmpty(request.DeviceId))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.LoginResponse,
                Payload = JsonSerializer.Serialize(new LoginResponse { Error = "Invalid login request." })
            });
            return;
        }

        playerId = "P_" + request.DeviceId;

        // Check for already-connected player
        if (!context.ConnectedPlayers.TryAdd(playerId, socket))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.LoginResponse,
                Payload = JsonSerializer.Serialize(new LoginResponse { Error = "Already connected." })
            });
            return;
        }

        // Here is where you use PlayerService:
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