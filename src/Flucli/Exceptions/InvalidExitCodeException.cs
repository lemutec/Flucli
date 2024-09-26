using System;

namespace Flucli.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an invalid exit code is returned by a process.
/// </summary>
public class InvalidExitCodeException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="InvalidExitCodeException" />.
    /// </summary>
    /// <param name="validExitCodes">The valid exit codes for the process.</param>
    /// <param name="message">The message of the exception.</param>
    public InvalidExitCodeException(int[]? validExitCodes = null, string message = "Invalid ExitCode") : base(message)
    {
        ValidExitCodes = validExitCodes ?? [0];
    }

    /// <summary>
    /// Gets the array containing all valid exit codes.
    /// </summary>
    public int[] ValidExitCodes { get; }
}
