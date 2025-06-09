namespace Shared.Messages;

public class UpdateResourcesRequest
{
    public string PlayerId { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public int ResourceValue { get; set; }
}