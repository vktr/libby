using Libby.Contracts;
using MassTransit;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Libby.Consumers;

public sealed class FilesConsumer(ILogger<FilesConsumer> logger) : IConsumer<GetFiles>
{
    public async Task Consume(ConsumeContext<GetFiles> context)
    {
        var matcher = new Matcher();
        matcher.AddIncludePatterns(context.Message.IncludePatterns);

        var result = matcher.Execute(
            new DirectoryInfoWrapper(
                new DirectoryInfo(context.Message.Path)));

        var files = result.Files
            .Select(f => Path.Join(context.Message.Path, f.Path))
            .ToList();

        logger.LogInformation("Found {FilesCount} file(s) in path {Path}", files.Count, context.Message.Path);

        await context.Publish(
            new GetFilesResult
            {
                CorrelationId = context.Message.CorrelationId,
                FilePaths = files
            });
    }
}
