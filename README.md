[![NuGet](https://img.shields.io/nuget/v/Flucli.svg)](https://nuget.org/packages/Flucli) [![GitHub license](https://img.shields.io/github/license/lemutec/Flucli)](https://github.com/lemutec/Flucli/blob/master/LICENSE) [![Actions](https://github.com/lemutec/Flucli/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/lemutec/Flucli/actions/workflows/library.nuget.yml)

# Flucli

Library for interacting with external command-line interfaces.

Ported from [CliWrap](https://github.com/Tyrrrz/CliWrap) and support `.netstandard 2.0` without other dependencies.

Support `Verb="runas"` and simplify some APIs.

## Usage

### Command

```c#
CliResult result = await "cmd"
    .WithArguments("/c echo Hello World!")
    .ExecuteAsync();

Console.WriteLine("ExitCode is " + result.ExitCode);
```

### Piper

```c#
StringBuilder stdout = new();
StringBuilder stderr = new();

var command1 = "cmd"
    .WithArguments("/c echo Hello World!");

var command2 = "cmd"
    .WithArguments("/c findstr o")
    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout, Encoding.UTF8))
    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr, Encoding.UTF8));

CliResult result = await (command1 | command2).ExecuteAsync();

Console.WriteLine("STDOUT: " + stdout.ToString());
Console.WriteLine("STDERR: " + stderr.ToString());
Console.WriteLine("ExitCode is " + result.ExitCode);
```

### Parser

```c#
StringBuilder stdout = new();
StringBuilder stderr = new();

Cli command = "cmd /c echo Follow | cmd /c findstr F | cmd /c findstr l*"
    .ParseCli()
    .PipeTail // Switch to tail command
    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout, Encoding.UTF8))
    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr, Encoding.UTF8))
    .PipeHeader; // Switch to header command

CliResult result = await command.ExecutePipeAsync();

Console.WriteLine("STDOUT: " + stdout.ToString());
Console.WriteLine("STDERR: " + stderr.ToString());
Console.WriteLine("ExitCode is " + result.ExitCode);
```

