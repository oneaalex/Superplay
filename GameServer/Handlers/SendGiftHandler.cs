using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Shared.Messages;

namespace GameServer.Handlers;

public class SendGiftHandler : IMessageHandler
{
    public MessageType MessageType => MessageType.SendGift;

    public async Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId)
    {
        // TODO: Implement send gift logic
        await Send(socket, new SocketMessage
        {
            Type = MessageType.GiftEvent,
            Payload = JsonSerializer.Serialize(new GiftEvent { FromPlayerId = "TODO" })
        });
    }

    private static async Task Send(WebSocket socket, SocketMessage msg)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

}