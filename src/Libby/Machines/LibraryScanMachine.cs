using Libby.Contracts;
using Libby.Data.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Libby.Machines;

public sealed class LibraryScanDefinition : SagaDefinition<LibraryScan>
{
    public LibraryScanDefinition()
    {
        Endpoint(e => e.PrefetchCount = 20);
    }

    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<LibraryScan> sagaConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseDelayedRedelivery(cfg =>
        {
            cfg.Handle<InvalidOperationException>(e => e.InnerException is DbUpdateException);
            cfg.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(13));
        });

        endpointConfigurator.UseMessageRetry(cfg =>
        {
            cfg.Handle<InvalidOperationException>(e => e.InnerException is DbUpdateException);
            cfg.Interval(10, TimeSpan.FromMilliseconds(100));
        });

        var partition = endpointConfigurator.CreatePartitioner(20);

        sagaConfigurator.Message<LibraryCreated>(x => x.UsePartitioner(partition, m => m.Message.LibraryId));
        sagaConfigurator.Message<GetFilesResult>(x => x.UsePartitioner(partition, m => m.Message.CorrelationId));
        sagaConfigurator.Message<LibraryItemImported>(x => x.UsePartitioner(partition, m => m.Message.LibraryId));
    }
}

public sealed class LibraryScanMachine : MassTransitStateMachine<LibraryScan>
{
    public LibraryScanMachine(ILogger<LibraryScanMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => OnLibraryCreated, x => x.CorrelateById(m => m.Message.LibraryId));
        Event(() => OnGetFilesResult, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OnLibraryItemImported, x => x.CorrelateById(m => m.Message.LibraryId));

        Initially(
            When(OnLibraryCreated)
                .Then(ctx => ctx.Saga.StatusDate = DateTimeOffset.UtcNow)
                .Publish(ctx => new GetFiles
                {
                    CorrelationId = ctx.Message.LibraryId,
                    Path = ctx.Message.LibraryPath,
                    IncludePatterns = ["**/*.mkv"]
                })
                .TransitionTo(GettingFiles));

        During(
            GettingFiles,
            When(OnGetFilesResult)
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.ItemsImporting ??= [];
                    ctx.Saga.StatusDate = DateTimeOffset.UtcNow;

                    foreach (var filePath in ctx.Message.FilePaths)
                    {
                        var id = Guid.NewGuid();
                        ctx.Saga.ItemsImporting.Add(id);

                        await ctx.Publish(
                            new ImportLibraryItem
                            {
                                CorrelationId = id,
                                LibraryId = ctx.Message.CorrelationId,
                                FilePath = filePath,
                            });
                    }
                })
                .TransitionTo(ImportingItems));

        During(
            ImportingItems,
            When(OnLibraryItemImported)
                .Then(ctx =>
                {
                    ctx.Saga.ItemsImported ??= [];
                    ctx.Saga.ItemsImported.Add(ctx.Message.CorrelationId);
                    ctx.Saga.StatusDate = DateTimeOffset.UtcNow;
                })
                .If(AllItemsImported, activity => activity.Finalize()));

        Finally(activity => activity
            .Then(_ => logger.LogInformation("All items imported")));
    }

    public Event<LibraryCreated> OnLibraryCreated { get; set; } = default!;

    public Event<GetFilesResult> OnGetFilesResult { get; set; } = default!;

    public Event<LibraryItemImported> OnLibraryItemImported { get; set; } = default!;

    public State GettingFiles { get; set; } = default!;

    public State ImportingItems { get; set; } = default!;

    private static bool AllItemsImported(BehaviorContext<LibraryScan, LibraryItemImported> context)
        => context.Saga.ItemsImporting?.All(i => context.Saga.ItemsImported?.Contains(i) is true) is true;

}
