using System.Net.WebSockets;
using Shared.Messages;

namespace GameServer.Handlers;

public interface IMessageHandler
{
    Task HandleAsync(WebSocket socket, SocketMessage message);
}