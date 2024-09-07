using Flucli.Utils;
using Flucli.Utils.Extensions;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

public partial class PipeTarget
{
    private class AnonymousPipeTarget(Func<Stream, CancellationToken, Task> copyFromAsync) : PipeTarget
    {
        public override async Task CopyFromAsync(Stream origin, CancellationToken cancellationToken = default)
            => await copyFromAsync(origin, cancellationToken).ConfigureAwait(false);
    }

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
        });

    public static PipeTarget ToStream(Stream stream, bool autoFlush)
        => Create(async (origin, cancellationToken) =>
                await origin.CopyToAsync(stream, autoFlush, cancellationToken).ConfigureAwait(false)
        );

    public static PipeTarget ToStream(Stream stream)
        => ToStream(stream, true);

    public static PipeTarget ToFile(string filePath)
        => Create(async (origin, cancellationToken) =>
        {
            var target = File.Create(filePath);
            using (target)
            {
                await origin.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
            }
        });

    public static PipeTarget ToStringBuilder(StringBuilder stringBuilder, Encoding encoding)
        => Create(async (origin, cancellationToken) =>
        {
            using var reader = new StreamReader(
                origin,
                encoding,
                false,
                BufferSizes.StreamReader,
                true
            );
            var buffer = new char[BufferSizes.StreamReader];

            while (true)
            {
                var charsRead = await reader
                    .ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .ConfigureAwait(false);
                if (charsRead <= 0)
                {
                    break;
                }

                stringBuilder.Append(buffer, 0, charsRead);
            }
        });

    public static PipeTarget ToStringBuilder(StringBuilder stringBuilder)
        => ToStringBuilder(stringBuilder, Console.OutputEncoding);

    public static PipeTarget ToDelegate(Func<string, CancellationToken, Task> handleLineAsync, Encoding encoding)
        => Create(async (origin, cancellationToken) =>
        {
            using var reader = new StreamReader(
                origin,
                encoding,
                false,
                BufferSizes.StreamReader,
                true
            );
            var lineBuffer = new StringBuilder();
            var buffer = new char[BufferSizes.StreamReader];
            var isLastCaretReturn = false;

            while (true)
            {
                var charsRead = await reader
                    .ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .ConfigureAwait(false);
                if (charsRead <= 0)
                {
                    break;
                }

                for (var i = 0; i < charsRead; i++)
                {
                    var c = buffer[i];

                    // If the current char and the last char are part of a line break sequence,
                    // skip over the current char and move on.
                    // The buffer was already yielded in the previous iteration, so there's
                    // nothing left to do.
                    if (isLastCaretReturn && c == '\n')
                    {
                        isLastCaretReturn = false;
                        continue;
                    }

                    // If the current char is \n or \r, yield the buffer (even if it is empty)
                    if (c is '\n' or '\r')
                    {
                        await handleLineAsync(lineBuffer.ToString(), cancellationToken).ConfigureAwait(false);
                        lineBuffer.Clear();
                    }
                    // For any other char, just append it to the buffer
                    else
                    {
                        lineBuffer.Append(c);
                    }

                    isLastCaretReturn = c == '\r';
                }
            }

            if (lineBuffer.Length > 0)
            {
                await handleLineAsync(lineBuffer.ToString(), cancellationToken).ConfigureAwait(false);
            }
        });

    public static PipeTarget ToDelegate(Func<string, CancellationToken, Task> handleLineAsync)
        => ToDelegate(handleLineAsync, Console.OutputEncoding);

    public static PipeTarget ToDelegate(Func<string, Task> handleLineAsync, Encoding encoding)
        => ToDelegate(async (line, _) => await handleLineAsync(line).ConfigureAwait(false), encoding);

    public static PipeTarget ToDelegate(Func<string, Task> handleLineAsync)
        => ToDelegate(handleLineAsync, Console.OutputEncoding);

    public static PipeTarget ToDelegate(Action<string> handleLine, Encoding encoding)
        => ToDelegate(line =>
            {
                handleLine(line);
                return Task.CompletedTask;
            },
            encoding
        );

    public static PipeTarget ToDelegate(Action<string> handleLine)
        => ToDelegate(handleLine, Console.OutputEncoding);
}

public abstract partial class PipeTarget
{
    public abstract Task CopyFromAsync(Stream origin, CancellationToken cancellationToken = default);
}
