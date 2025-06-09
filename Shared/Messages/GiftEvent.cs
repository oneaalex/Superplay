namespace Shared.Messages;

public class GiftEvent
{
    public string FromPlayerId { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public int ResourceValue { get; set; }
}