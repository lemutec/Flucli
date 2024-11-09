using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli.Utils.Extensions;

internal static class TaskExtensions
{
    /// <summary>
    /// Waits for the specified task to complete within the given timeout period or throws a TimeoutException if it doesn't.
    /// Also accepts a CancellationToken to allow task cancellation.
    /// </summary>
    /// <param name="task">The task to wait on.</param>
    /// <param name="timeout">The maximum time to wait for the task to complete.</param>
    /// <param name="cancellationToken">The token to observe for cancellation.</param>
    /// <exception cref="TimeoutException">Thrown if the task does not complete within the specified timeout.</exception>
    public static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken)
    {
        Task cancellationTask = Task.Delay(timeout, cancellationToken);
        Task finishedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);

        await finishedTask.ConfigureAwait(false);

        if (finishedTask == cancellationTask)
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }

    /// <summary>
    /// Waits for the specified task to complete, allowing cancellation via the CancellationToken.
    /// </summary>
    /// <param name="task">The task to wait on.</param>
    /// <param name="cancellationToken">The token to observe for cancellation.</param>
    public static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
        => await task.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Waits for the specified task to complete within the given timeout period.
    /// </summary>
    /// <param name="task">The task to wait on.</param>
    /// <param name="timeout">The maximum time to wait for the task to complete.</param>
    /// <exception cref="TimeoutException">Thrown if the task does not complete within the specified timeout.</exception>
    public static async Task WaitAsync(this Task task, TimeSpan timeout)
        => await task.WaitAsync(timeout, CancellationToken.None).ConfigureAwait(false);
}
