using Serilog;
using Shared.Messages;
using System.Net.WebSockets;

namespace GameServer.Handlers;

public class WebSocketServer
{
    private readonly HandlerRegistry _handlerRegistry;
    private readonly GameServerContext _context;

    public WebSocketServer(HandlerRegistry handlerRegistry, GameServerContext context)
    {
        _handlerRegistry = handlerRegistry;
        _context = context;
    }

    public async Task ProcessMessageAsync(SocketMessage message, WebSocket socket)
    {
        if (_handlerRegistry.TryGetHandler(message.Type, out var handler))
        {
            if (handler == null)
            {
                Log.Error("Handler for message type {MessageType} not found.", message.Type);
                return;
            }
            await handler.HandleAsync(message, socket, _context, null);
        }
        else
        {
            Log.Error("No handler registered for message type {MessageType}", message.Type);
        }
    }
}