namespace Shared.Models;

using Shared.Messages;

public class PlayerState
{
    public string PlayerId { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public Dictionary<ResourceType, int> Resources { get; set; } = new()
    {
        { ResourceType.Coins, 0 },
        { ResourceType.Rolls, 0 }
    };
}