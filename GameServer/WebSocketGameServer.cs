using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Shared.Messages;
using GameServer.Services;
using GameServer.Handlers;

public class WebSocketGameServer
{
    private readonly int _port;
    private readonly PlayerService _playerService = new();
    private readonly Dictionary<MessageType, IMessageHandler> _handlers;

    public WebSocketGameServer(int port)
    {
        _port = port;

        // Register handlers for each message type
        _handlers = new()
        {
            { MessageType.Login, new LoginHandler(_playerService) }
            // Add additional handlers here
        };
    }

    public async Task StartAsync()
    {
        HttpListener listener = new();
        listener.Prefixes.Add($"http://localhost:{_port}/ws/");
        listener.Start();
        Log.Information("Server listening on ws://localhost:{Port}/ws/", _port);

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                _ = HandleConnectionAsync(context);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task HandleConnectionAsync(HttpListenerContext context)
    {
        WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
        WebSocket socket = wsContext.WebSocket;

        Log.Information("New connection established");

        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Log.Information("Received: {Message}", message);

                var socketMessage = JsonSerializer.Deserialize<SocketMessage>(message);
                if (socketMessage != null)
                {
                    await RouteMessage(socket, socketMessage);
                }
            }
        }
    }

    private async Task RouteMessage(WebSocket socket, SocketMessage socketMessage)
    {
        if (_handlers.TryGetValue(socketMessage.Type, out var handler))
        {
            await handler.HandleAsync(socket, socketMessage);
        }
        else
        {
            Log.Warning("No handler registered for message type {Type}", socketMessage.Type);
        }
    }
}