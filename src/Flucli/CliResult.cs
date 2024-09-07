using System;

namespace Flucli;

public class CliResult(int exitCode, DateTimeOffset startTime, DateTimeOffset exitTime)
{
    public int ExitCode { get; } = exitCode;

    public bool IsSuccess => ExitCode == 0;

    public DateTimeOffset StartTime { get; } = startTime;

    public DateTimeOffset ExitTime { get; } = exitTime;

    public TimeSpan RunTime => ExitTime - StartTime;
}
