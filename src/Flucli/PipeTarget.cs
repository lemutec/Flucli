using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Flucli;

public partial class PipeTarget
{
    public static PipeTarget Null { get; }
        = Create((_, cancellationToken) =>
            !cancellationToken.IsCancellationRequested
                ? Task.CompletedTask
                : Task.FromCanceled(cancellationToken)
        );

    public static PipeTarget Create(Func<Stream, CancellationToken, Task> handlePipeAsync)
        => new AnonymousPipeTarget(handlePipeAsync);

    public static PipeTarget Create(Action<Stream> handlePipe)
        => Create((origin, _) =>
        {
            handlePipe(origin);
            return Task.CompletedTask;
        }
    );

    public static PipeTarget ToStream(Stream stream, bool autoFlush)
        => Create(async (origin, cancellationToken) =>
                await origin.CopyToAsync(stream, autoFlush, cancellationToken).ConfigureAwait(false)
        );

    public static PipeTarget ToStream(Stream stream)
        => ToStream(stream, true);

    private class AnonymousPipeTarget(Func<Stream, CancellationToken, Task> copyFromAsync) : PipeTarget
    {
        public override async Task CopyFromAsync(Stream origin, CancellationToken cancellationToken = default)
            => await copyFromAsync(origin, cancellationToken).ConfigureAwait(false);
    }
}

public abstract partial class PipeTarget
{
    public abstract Task CopyFromAsync(Stream origin, CancellationToken cancellationToken = default);
}
