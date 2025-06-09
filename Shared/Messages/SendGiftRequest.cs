namespace Shared.Messages;

public class SendGiftRequest
{
    public string PlayerId { get; set; } = string.Empty;
    public string FriendPlayerId { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public int ResourceValue { get; set; }
}