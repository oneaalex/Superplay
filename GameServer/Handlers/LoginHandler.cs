using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Shared.Messages;
using GameServer.Services;

namespace GameServer.Handlers;

public class LoginHandler : IMessageHandler
{
    private readonly PlayerService _playerService;

    public LoginHandler(PlayerService playerService)
    {
        _playerService = playerService;
    }

    public async Task HandleAsync(WebSocket socket, SocketMessage message)
    {
        var loginRequest = JsonSerializer.Deserialize<LoginRequest>(message.Payload);
        var response = _playerService.HandleLogin(loginRequest?.DeviceId ?? "", socket);

        var responseMessage = new SocketMessage
        {
            Type = MessageType.Login,
            Payload = JsonSerializer.Serialize(response)
        };

        var json = JsonSerializer.Serialize(responseMessage);
        var bytes = Encoding.UTF8.GetBytes(json);
        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}