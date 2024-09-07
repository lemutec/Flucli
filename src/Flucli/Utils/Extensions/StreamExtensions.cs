using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

internal static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, bool autoFlush, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[81920];

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
}
