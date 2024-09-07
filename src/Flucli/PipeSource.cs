using Flucli.Utils.Extensions;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

public partial class PipeSource
{
    private class AnonymousPipeSource(Func<Stream, CancellationToken, Task> copyToAsync) : PipeSource
    {
        public override async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
            => await copyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }

    public static PipeSource Null { get; }
        = Create((_, cancellationToken) =>
            !cancellationToken.IsCancellationRequested
                ? Task.CompletedTask
                : Task.FromCanceled(cancellationToken)
        );

    public static PipeSource Create(Func<Stream, CancellationToken, Task> handlePipeAsync)
        => new AnonymousPipeSource(handlePipeAsync);

    public static PipeSource Create(Action<Stream> handlePipe)
        => Create((destination, _) =>
        {
            handlePipe(destination);
            return Task.CompletedTask;
        });

    public static PipeSource FromStream(Stream stream, bool autoFlush)
        => Create(async (destination, cancellationToken) =>
            await stream
                .CopyToAsync(destination, autoFlush, cancellationToken)
                .ConfigureAwait(false)
        );

    public static PipeSource FromFile(string filePath)
        => Create(async (destination, cancellationToken) =>
            {
                var source = File.OpenRead(filePath);
                using (source)
                {
                    await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
                }
            }
        );

    public static PipeSource FromBytes(byte[] data)
        => Create(async (destination, cancellationToken) =>
              await destination.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false)
      );

    public static PipeSource FromString(string str, Encoding encoding)
        => FromBytes(encoding.GetBytes(str));

    public static PipeSource FromString(string str)
        => FromString(str, Console.InputEncoding);

    public static PipeSource FromCli(Cli cli)
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
