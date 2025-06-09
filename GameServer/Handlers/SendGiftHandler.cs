using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Shared.Messages;

namespace GameServer.Handlers;

public class SendGiftHandler : IMessageHandler
{
    public MessageType MessageType => MessageType.SendGift;

    public async Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId)
    {
        var request = JsonSerializer.Deserialize<SendGiftRequest>(message.Payload);
        if (request == null ||
            string.IsNullOrWhiteSpace(request.PlayerId) ||
            string.IsNullOrWhiteSpace(request.FriendPlayerId) ||
            request.ResourceValue <= 0)
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.GiftEvent,
                Payload = JsonSerializer.Serialize(new GiftEvent
                {
                    FromPlayerId = request?.PlayerId ?? "",
                    ResourceType = request?.ResourceType ?? 0,
                    ResourceValue = 0
                })
            });
            return;
        }

        // Transfer resource
        if (!context.PlayerService.TransferResource(request.PlayerId, request.FriendPlayerId, request.ResourceType, request.ResourceValue))
        {
            await Send(socket, new SocketMessage
            {
                Type = MessageType.GiftEvent,
                Payload = JsonSerializer.Serialize(new GiftEvent
                {
                    FromPlayerId = request.PlayerId,
                    ResourceType = request.ResourceType,
                    ResourceValue = 0
                })
            });
            return;
        }

        // Notify sender
        await Send(socket, new SocketMessage
        {
            Type = MessageType.GiftEvent,
            Payload = JsonSerializer.Serialize(new GiftEvent
            {
                FromPlayerId = request.PlayerId,
                ResourceType = request.ResourceType,
                ResourceValue = request.ResourceValue
            })
        });

        // Notify friend if online
        if (context.ConnectedPlayers.TryGetValue(request.FriendPlayerId, out var friendSocket))
        {
            await Send(friendSocket, new SocketMessage
            {
                Type = MessageType.GiftEvent,
                Payload = JsonSerializer.Serialize(new GiftEvent
                {
                    FromPlayerId = request.PlayerId,
                    ResourceType = request.ResourceType,
                    ResourceValue = request.ResourceValue
                })
            });
        }
    }

    private static async Task Send(WebSocket socket, SocketMessage msg)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}