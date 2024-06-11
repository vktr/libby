using System.Text;
using System.Text.Json;
using CliWrap;
using Libby.Contracts;
using Libby.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Libby.Consumers;

public sealed class LibraryItemsConsumer(
    ILogger<LibraryItemsConsumer> logger,
    LibbyDataContext dataContext) : IConsumer<ImportLibraryItem>
{
    public async Task Consume(ConsumeContext<ImportLibraryItem> context)
    {
        var library = await dataContext.Libraries
            .Where(l => l.Id == context.Message.LibraryId)
            .SingleOrDefaultAsync(context.CancellationToken);

        if (library is null)
        {
            logger.LogError("Library not found");
            throw new ApplicationException();
        }

        var fileInfo = new FileInfo(context.Message.FilePath);

        var stdErr = new StringBuilder();
        var stdOut = new StringBuilder();

        var result = await Cli.Wrap("/usr/bin/ffprobe")
            .WithArguments(args => args
                // Verbosity
                .Add("-v")
                .Add("warning")
                // Output json
                .Add("-print_format")
                .Add("json")
                // Analyze duration
                .Add("-analyzeduration")
                .Add("200M")
                // Probe size
                .Add("-probesize")
                .Add("1G")
                // Show chapters
                .Add("-show_chapters")
                // Show formats
                .Add("-show_format")
                // Show streams
                .Add("-show_streams")
                // Threads
                .Add("-threads")
                .Add("0")
                // Input file
                .Add("-i")
                .Add(fileInfo.FullName))
            .WithStandardErrorPipe(
                PipeTarget.ToStringBuilder(stdErr))
            .WithStandardOutputPipe(
                PipeTarget.ToStringBuilder(stdOut))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        logger.LogInformation(
            "ffprobe finished with exit code {ExitCode} in {Duration}",
            result.ExitCode,
            result.RunTime);

        var item = new Data.Models.LibraryItem
        {
            Id = Guid.NewGuid(),
            Library = library,
            FileName = fileInfo.FullName,
            FileSize = fileInfo.Length,
            FfprobeDate = DateTimeOffset.UtcNow,
            FfprobeJson = stdOut.Length > 0
                ? JsonDocument.Parse(stdOut.ToString())
                : null,
            FFprobeError = stdErr.Length > 0
                ? stdErr.ToString()
                : null
        };

        await dataContext.LibraryItems.AddAsync(item);

        await context.Publish(
            new LibraryItemImported
            {
                CorrelationId = context.Message.CorrelationId,
                LibraryId = context.Message.LibraryId,
                LibraryItemId = item.Id
            });

        await dataContext.SaveChangesAsync();
    }
}
