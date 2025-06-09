using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Shared.Messages;

namespace GameServer.Handlers;

public class UpdateResourcesHandler : IMessageHandler
{
    public MessageType MessageType => MessageType.UpdateResources;

    public async Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId)
    {
        var request = JsonSerializer.Deserialize<UpdateResourcesRequest>(message.Payload);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.UpdateResourcesResponse,
                Payload = JsonSerializer.Serialize(new UpdateResourcesResponse { Error = "Invalid request." })
            });
            return;
        }

        if (!context.PlayerService.UpdateResource(request.PlayerId, request.ResourceType, request.ResourceValue, out int newBalance))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.UpdateResourcesResponse,
                Payload = JsonSerializer.Serialize(new UpdateResourcesResponse { Error = "Player not found or invalid resource." })
            });
            return;
        }

        await Send(socket, new SocketMessage
        {
            Type = MessageType.UpdateResourcesResponse,
            Payload = JsonSerializer.Serialize(new UpdateResourcesResponse
            {
                PlayerId = request.PlayerId,
                ResourceType = request.ResourceType,
                NewBalance = newBalance
            })
        });
    }

    private static async Task Send(WebSocket socket, SocketMessage msg)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}