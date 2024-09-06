using System;
using System.Collections.Generic;

namespace Flucli;

public class Cli : ICli
{
    private string _fileName = string.Empty;
    public string FileName => _fileName;

    private string _arguments = string.Empty;
    public string Arguments => _arguments;

    private string _workingDirPath = string.Empty;
    public string WorkingDirPath => _workingDirPath;

    private IReadOnlyDictionary<string, string?> _environmentVariables = new Dictionary<string, string?>();
    public IReadOnlyDictionary<string, string?> EnvironmentVariables => _environmentVariables;

    public PipeSource _standardInputPipe => null!;
    public PipeSource StandardInputPipe => _standardInputPipe;

    public PipeTarget _standardOutputPipe => null!;
    public PipeTarget StandardOutputPipe => _standardOutputPipe;

    public PipeTarget _standardErrorPipe => null!;
    public PipeTarget StandardErrorPipe => _standardErrorPipe;

    public Cli() : this(default(string)!)
    {
    }

    public Cli(string fileName = null!)
    {
        _fileName = fileName?.Trim()!;
    }

    public Cli(Uri uri)
    {
        _fileName = (uri ?? throw new ArgumentNullException(nameof(uri))).OriginalString;
    }

    public Cli WithArguments(params string[] args)
    {
        foreach (string arg in args)
        {
            WithArgument(arg);
        }

        return this;
    }

    public Cli WithArguments(IEnumerable<string> args)
    {
        foreach (string arg in args)
        {
            WithArgument(arg);
        }

        return this;
    }

    public Cli WithArgument(string arg)
    {
        // TODO
        return this;
    }

    //public static Cli operator |(Cli source, PipeTarget target)
    //    => source.WithStandardOutputPipe(target);

    //public static Cli operator |(Cli source, Stream target)
    //    => source | PipeTarget.ToStream(target);

    //public static Cli operator |(Cli source, (Stream stdOut, Stream stdErr) targets)
    //    => source | (PipeTarget.ToStream(targets.stdOut), PipeTarget.ToStream(targets.stdErr));

    //public static Cli operator |(PipeSource source, Cli target) =>
    //    target.WithStandardInputPipe(source);

    //public static Cli operator |(Stream source, Cli target) =>
    //    PipeSource.FromStream(source) | target;

    //public static Cli operator |(Cli source, Cli target) =>
    //    PipeSource.FromCommand(source) | target;
}

public interface ICli
{
    public string FileName { get; }
    public string Arguments { get; }
    public string WorkingDirPath { get; }

    public IReadOnlyDictionary<string, string?> EnvironmentVariables { get; }

    public PipeSource StandardInputPipe { get; }

    public PipeTarget StandardOutputPipe { get; }

    public PipeTarget StandardErrorPipe { get; }
}

public class PipeSource
{
}

public class PipeTarget
{
}

public class CliResult(int exitCode, DateTimeOffset startTime, DateTimeOffset exitTime)
{
    public int ExitCode { get; } = exitCode;

    public bool IsSuccess => ExitCode == 0;

    public DateTimeOffset StartTime { get; } = startTime;

    public DateTimeOffset ExitTime { get; } = exitTime;

    public TimeSpan RunTime => ExitTime - StartTime;
}
