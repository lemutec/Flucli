namespace Flucli.Utils;

internal static class BufferSizes
{
    /// <summary>
    /// Default buffer size for general stream operations, typically used to optimize data transfer.
    /// </summary>
    public const int Stream = 81920;

    /// <summary>
    /// Default buffer size for StreamReader operations, typically used for reading smaller chunks of data efficiently.
    /// </summary>
    public const int StreamReader = 1024;
}
