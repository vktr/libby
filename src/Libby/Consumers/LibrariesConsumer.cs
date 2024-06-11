using Libby.Contracts;
using MassTransit;

namespace Libby.Consumers;

public sealed class LibrariesConsumer : IConsumer<LibraryCreated>
{
    public Task Consume(ConsumeContext<LibraryCreated> context)
    {
        return Task.CompletedTask;
    }
}