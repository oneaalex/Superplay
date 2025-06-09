using System.Net.WebSockets;
using System.Threading.Tasks;
using Shared.Messages;

namespace GameServer.Handlers;

public interface IMessageHandler
{
    MessageType MessageType { get; }
    Task HandleAsync(SocketMessage message, WebSocket socket, GameServerContext context, string? playerId);
}
