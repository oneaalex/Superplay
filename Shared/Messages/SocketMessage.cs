namespace Shared.Messages;

public class SocketMessage
{
    public MessageType Type { get; set; }
    public string Payload { get; set; } = string.Empty;
}