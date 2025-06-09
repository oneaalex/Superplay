using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Shared.Messages;

namespace GameServer.Handlers;

public class UpdateResourcesHandler : IMessageHandler
{
    public MessageType MessageType => MessageType.UpdateResources;

    public async Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId)
    {
        // TODO: Implement resource update logic
        await Send(socket, new SocketMessage
        {
            Type = MessageType.UpdateResourcesResponse,
            Payload = JsonSerializer.Serialize(new UpdateResourcesResponse { Error = "Not implemented." })
        });
    }

    private static async Task Send(WebSocket socket, SocketMessage msg)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}