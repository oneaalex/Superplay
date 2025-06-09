using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Shared.Messages;
using System.Collections.Concurrent;

namespace GameServer;

public class WebSocketGameServer
{
    private readonly HttpListener _httpListener;
    private readonly ConcurrentDictionary<string, WebSocket> _connectedClients = new();
    private readonly int _port;

    public WebSocketGameServer(int port)
    {
        _port = port;
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

                // Route message by type (to handler, to be implemented in next step)
                await RouteMessageAsync(socketMsg, webSocket, playerId);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error handling client.");
        }
        finally
        {
            if (playerId != null)
            {
                _connectedClients.TryRemove(playerId, out _);
            }

            if (webSocket != null)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Log.Information("WebSocket connection closed.");
        }
    }

    // Placeholder for routing logic. To be implemented next step.
    private async Task RouteMessageAsync(SocketMessage socketMsg, WebSocket webSocket, string? playerId)
    {
        // Example: Just echo back for now.
        var response = JsonSerializer.Serialize(socketMsg);
        await webSocket.SendAsync(
            Encoding.UTF8.GetBytes(response),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
}