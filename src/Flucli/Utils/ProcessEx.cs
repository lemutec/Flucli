using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

    // Sends SIGINT
    public void Interrupt()
    {
        if (!TryInterrupt())
        {
            Kill();
            Debug.Fail("Failed to send an interrupt signal.");
        }

        bool TryInterrupt()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (_nativeProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        return _nativeProcess.CloseMainWindow();
                    }

                    // TODO: Find a way to send Ctrl+C to the console window
                    return true;
                }

                // On Unix, we can just send the signal to the process directly
                if (
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                )
                {
                    return NativeMethods.Unix.Kill(_nativeProcess.Id, 2) == 0;
                }

                // Unsupported platform
                return false;
            }
            catch
            {
                return false;
            }
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

file static class NativeMethods
{
    public static class Windows
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

        public delegate bool ConsoleCtrlDelegate(uint dwCtrlEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? handlerRoutine, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
    }

    public static class Unix
    {
        [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
        public static extern int Kill(int pid, int sig);
    }
}
