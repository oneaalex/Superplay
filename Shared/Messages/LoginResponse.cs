namespace Shared.Messages;

public class LoginResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public string? Error { get; set; }
}