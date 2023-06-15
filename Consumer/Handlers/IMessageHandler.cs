using Contracts;

namespace Consumer.Handlers;

public interface IMessageHandler
{
    public Task HandleAsync(IMessage message);

#pragma warning disable CA2252 // This API requires opting into preview features
    public static abstract Type MessageType { get; }
#pragma warning restore CA2252 // This API requires opting into preview features
}
