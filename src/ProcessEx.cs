using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

internal sealed class ProcessEx(ProcessStartInfo startInfo) : IDisposable
{
    private readonly Process _nativeProcess = new() { StartInfo = startInfo };

    private readonly TaskCompletionSource<object?> _exitTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Id => _nativeProcess.Id;

    public string Name =>
        // Can't rely on ProcessName because it becomes inaccessible after the process exits
        Path.GetFileName(_nativeProcess.StartInfo.FileName);

    // We are purposely using Stream instead of StreamWriter/StreamReader to push the concerns of
    // writing and reading to PipeSource/PipeTarget at the higher level.

    public Stream StandardInput => _nativeProcess.StandardInput.BaseStream;

    public Stream StandardOutput => _nativeProcess.StandardOutput.BaseStream;

    public Stream StandardError => _nativeProcess.StandardError.BaseStream;

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset ExitTime { get; private set; }

    public int ExitCode => _nativeProcess.ExitCode;

    public void Start()
    {
        // Hook up events
        _nativeProcess.EnableRaisingEvents = true;
        _nativeProcess.Exited += (_, _) =>
        {
            ExitTime = DateTimeOffset.Now;
            _exitTcs.TrySetResult(null);
        };

        // Start the process
        try
        {
            if (!_nativeProcess.Start())
            {
                throw new InvalidOperationException(
                    $"Failed to start a process with file path '{_nativeProcess.StartInfo.FileName}'. "
                        + "Target file is not an executable or lacks execute permissions."
                );
            }

            StartTime = DateTimeOffset.Now;
        }
        catch (Win32Exception ex)
        {
            throw new Win32Exception(
                $"Failed to start a process with file path '{_nativeProcess.StartInfo.FileName}'. "
                    + "Target file or working directory doesn't exist, or the provided credentials are invalid.",
                ex
            );
        }
    }

    // Sends SIGKILL
    public void Kill()
    {
        try
        {
            _nativeProcess.Kill();
        }
        catch when (_nativeProcess.HasExited)
        {
            // The process has exited before we could kill it. This is fine.
        }
        catch
        {
            // The process either failed to exit or is in the process of exiting.
            // We can't really do anything about it, so just ignore the exception.
            Debug.Fail("Failed to kill the process.");
        }
    }

    public async Task WaitUntilExitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.Register(() => _exitTcs.TrySetCanceled(cancellationToken)).Dispose();
        await _exitTcs.Task.ConfigureAwait(false);
    }

    public void Dispose() => _nativeProcess.Dispose();
}
