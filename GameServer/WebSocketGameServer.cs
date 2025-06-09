using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Shared.Messages;
using GameServer.Handlers;

namespace GameServer;

public class WebSocketGameServer
{
    private readonly HttpListener _httpListener;
    private readonly int _port;
    private readonly HandlerRegistry _handlerRegistry;
    private readonly GameServerContext _context;

    public WebSocketGameServer(int port, HandlerRegistry handlerRegistry, GameServerContext context)
    {
        _port = port;
        _handlerRegistry = handlerRegistry;
        _context = context;
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://localhost:{_port}/ws/");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _httpListener.Start();
        Log.Information("Server listening on ws://localhost:{Port}/ws/", _port);

        while (!cancellationToken.IsCancellationRequested)
        {
            var httpContext = await _httpListener.GetContextAsync();
            if (httpContext.Request.IsWebSocketRequest)
            {
                _ = HandleClientAsync(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.Close();
            }
        }
    }

    private async Task HandleClientAsync(HttpListenerContext context)
    {
        WebSocket webSocket = null!;
        string? playerId = null;
        try
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            webSocket = wsContext.WebSocket;

            Log.Information("New WebSocket connection established.");

            var buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Log.Debug("Received message: {Json}", json);

                var socketMsg = JsonSerializer.Deserialize<SocketMessage>(json);
                if (socketMsg == null)
                {
                    Log.Warning("Invalid message format");
                    continue;
                }

                await RouteMessageAsync(socketMsg, webSocket, playerId);
            }
        }
        catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
        {
            Log.Information("Client disconnected gracefully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error handling client.");
        }
        finally
        {
            if (playerId != null)
            {
                _context.ConnectedPlayers.TryRemove(playerId, out _);
            }

            if (webSocket != null)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Log.Information("WebSocket connection closed.");
        }
    }

    private async Task RouteMessageAsync(SocketMessage socketMsg, WebSocket webSocket, string? playerId)
    {
        if (socketMsg == null)
        {
            Log.Warning("Received a null SocketMessage.");
            return;
        }

        if (_handlerRegistry.TryGetHandler(socketMsg.Type, out var handler))
        {
            if (handler != null) // Ensure handler is not null
            {
                await handler.HandleAsync(socketMsg, webSocket, _context, playerId);
            }
            else
            {
                Log.Warning("Handler retrieved from registry is null for message type: {Type}", socketMsg.Type);
            }
        }
        else
        {
            Log.Warning("No handler for message type: {Type}", socketMsg.Type);
            var errorMsg = new SocketMessage
            {
                Type = MessageType.Error,
                Payload = JsonSerializer.Serialize(new { Error = "Unknown message type." })
            };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorMsg));
            await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}