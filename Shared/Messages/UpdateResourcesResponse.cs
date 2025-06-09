namespace Shared.Messages;

public class UpdateResourcesResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public int NewBalance { get; set; }
    public string? Error { get; set; }
}