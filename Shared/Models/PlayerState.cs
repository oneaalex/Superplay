namespace Shared.Models;

using Shared.Messages;

public class PlayerState
{
    public string PlayerId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public Dictionary<ResourceType, int> Resources { get; set; } = new();
}