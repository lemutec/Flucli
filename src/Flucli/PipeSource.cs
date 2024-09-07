using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

public partial class PipeSource
{
    public static PipeSource Null { get; }
        = Create(
            (_, cancellationToken) =>
                !cancellationToken.IsCancellationRequested
                    ? Task.CompletedTask
                    : Task.FromCanceled(cancellationToken)
        );

    public static PipeSource Create(Func<Stream, CancellationToken, Task> handlePipeAsync)
        => new AnonymousPipeSource(handlePipeAsync);

    public static PipeSource Create(Action<Stream> handlePipe)
        => Create(
            (destination, _) =>
            {
                handlePipe(destination);
                return Task.CompletedTask;
            }
        );

    public static PipeSource FromStream(Stream stream, bool autoFlush)
        => Create(
            async (destination, cancellationToken) =>
                await stream
                    .CopyToAsync(destination, autoFlush, cancellationToken)
                    .ConfigureAwait(false)
        );

    private class AnonymousPipeSource(Func<Stream, CancellationToken, Task> copyToAsync) : PipeSource
    {
        public override async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
            => await copyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }

    public static PipeSource FromCommandLine(Cli cli)
        => Create(async (destination, cancellationToken) =>
            await cli
                .WithStandardOutputPipe(PipeTarget.ToStream(destination))
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false)
        );

    public static PipeSource FromStream(Stream stream) => FromStream(stream, true);
}

public abstract partial class PipeSource
{
    public abstract Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default);
}
