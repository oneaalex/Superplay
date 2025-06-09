using Shared.Messages;

namespace GameServer.Handlers;

public class HandlerRegistry
{
    private readonly Dictionary<MessageType, IMessageHandler> _handlers;

    public HandlerRegistry(IEnumerable<IMessageHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.MessageType, h => h);
    }

    public bool TryGetHandler(MessageType type, out IMessageHandler? handler)
        => _handlers.TryGetValue(type, out handler);
}
