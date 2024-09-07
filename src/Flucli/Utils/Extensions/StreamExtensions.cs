using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli.Utils.Extensions;

internal static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, bool autoFlush, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[BufferSizes.Stream];

        while (true)
        {
            int bytesRead = await source
                .ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                .ConfigureAwait(false);

            if (bytesRead <= 0)
                break;

            await destination
                .WriteAsync(buffer, 0, bytesRead, cancellationToken)
                .ConfigureAwait(false);

            if (autoFlush)
            {
                await destination
                    .FlushAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    public static async Task CopyToAsync(this Stream source, Stream destination, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[BufferSizes.Stream];

        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
            .ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task<int> ReadAsync(this StreamReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken = default)
    {
        _ = reader ?? throw new ArgumentNullException(nameof(reader));
        _ = buffer ?? throw new ArgumentNullException(nameof(buffer));

        if (index < 0 || count < 0 || index + count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException("Index and count must be within the bounds of the buffer.");
        }

        int totalCharsRead = 0;

        while (totalCharsRead < count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int charsRead = await reader.ReadAsync(buffer, index + totalCharsRead, count - totalCharsRead).ConfigureAwait(false);

            if (charsRead == 0)
            {
                break;
            }

            totalCharsRead += charsRead;
        }

        return totalCharsRead;
    }
}
