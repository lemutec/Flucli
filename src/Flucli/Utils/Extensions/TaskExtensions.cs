using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

internal static class TaskExtensions
{
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

    public static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
        => await task.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);

    public static async Task WaitAsync(this Task task, TimeSpan timeout)
        => await task.WaitAsync(timeout, CancellationToken.None).ConfigureAwait(false);
}
