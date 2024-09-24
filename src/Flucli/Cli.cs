using Flucli.Utils;
using Flucli.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flucli;

public class Cli : ICli
{
    private string _fileName = string.Empty;
    public string FileName => _fileName;

    private string _arguments = string.Empty;
    public string Arguments => _arguments;

    private string _workingDirectory = string.Empty;
    public string WorkingDirectory => _workingDirectory;

    private string? _domain = null;
    public string? Domain => _domain;

    private bool _useShellExecute = false;
    public bool UseShellExecute => _useShellExecute;

    private bool _createNoWindow = true;
    public bool CreateNoWindow => _createNoWindow;

    private string? _password = null;
    public string? Password => _password;

    private string _verb = string.Empty;
    public string Verb => _verb;

    private nint _errorDialogParentHandle = IntPtr.Zero;
    public nint ErrorDialogParentHandle => _errorDialogParentHandle;

    private bool _errorDialog = false;
    public bool ErrorDialog => _errorDialog;

    private string _userName = string.Empty;
    public string UserName => _userName ?? string.Empty;

    private ProcessWindowStyle _windowStyle = ProcessWindowStyle.Normal;
    public ProcessWindowStyle WindowStyle => _windowStyle;

    private bool _loadUserProfile = false;
    public bool LoadUserProfile => _loadUserProfile;

    private string? _passwordInClearText = default;
    public string? PasswordInClearText => _passwordInClearText;

    private Dictionary<string, string?> _environmentVariables = [];
    public Dictionary<string, string?> EnvironmentVariables => _environmentVariables;

    public PipeSource _standardInputPipe = PipeSource.Null;
    public PipeSource StandardInputPipe => _standardInputPipe;

    public PipeTarget _standardOutputPipe = PipeTarget.Null;
    public PipeTarget StandardOutputPipe => _standardOutputPipe;

    public PipeTarget _standardErrorPipe = PipeTarget.Null;
    public PipeTarget StandardErrorPipe => _standardErrorPipe;

    /// <summary>
    /// Only stock the cli parse result from <see cref="CliExtensions.ParseCli"/> here.
    /// **No automatic execution of pipe.**
    /// **You have to run it by yourself.**
    /// </summary>
    public Cli PipeTo { get; set; } = null!;

    public Cli PipeTail
    {
        get
        {
            Cli current = this;
            while (current != null && current.PipeTo != null)
            {
                current = current.PipeTo;
            }
            return current!;
        }
    }

    /// <summary>
    /// Only stock the cli parse result from <see cref="CliExtensions.ParseCli"/> here.
    /// </summary>
    public Cli PipeFrom { get; set; } = null!;

    public Cli PipeHeader
    {
        get
        {
            Cli current = this;
            while (current != null && current.PipeFrom != null)
            {
                current = current.PipeFrom;
            }
            return current!;
        }
    }

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

    public static Cli Wrap(string fileName)
    {
        return fileName.CreateCli();
    }

    public Cli WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public Cli WithArguments(params string[] arguments)
    {
        return WithArguments(arguments.ToArguments());
    }

    public Cli WithArguments(IEnumerable<string> arguments)
    {
        return WithArguments(arguments.ToArguments());
    }

    public Cli WithArguments(string arguments)
    {
        _arguments = arguments;
        return this;
    }

    public Cli WithWorkingDirectory(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        return this;
    }

    public Cli WithEnvironmentVariable(IEnumerable<(string, string?)> variables)
    {
        _environmentVariables = variables.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        return this;
    }

    public Cli AddEnvironmentVariable(string name, string value)
    {
        _environmentVariables.Add(name, value);
        return this;
    }

    public Cli RemoveEnvironmentVariable(string name)
    {
        _environmentVariables.Remove(name);
        return this;
    }

    public Cli WithCreateNoWindow(bool createNoWindow = true)
    {
        _createNoWindow = createNoWindow;
        return this;
    }

    public Cli WithUseShellExecute(bool useShellExecute)
    {
        _useShellExecute = useShellExecute;
        return this;
    }

    public Cli WithVerb(string verb)
    {
        _verb = verb;
        return this;
    }

    public Cli WithErrorDialog(bool errorDialog = true)
    {
        _errorDialog = errorDialog;
        return this;
    }

    public Cli WithErrorDialogParentHandle(nint parentWindowHandle)
    {
        _errorDialogParentHandle = parentWindowHandle;
        return this;
    }

    public Cli WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public Cli WithWindowStyle(ProcessWindowStyle windowStyle)
    {
        _windowStyle = windowStyle;
        return this;
    }

    [Description("Only supported on Windows")]
    public Cli WithDomain(string? domain)
    {
        _domain = domain;
        return this;
    }

    [Description("Only supported on Windows")]
    public Cli WithLoadUserProfile(bool loadUserProfile = true)
    {
        _loadUserProfile = loadUserProfile;
        return this;
    }

    [Description("Only supported on Windows")]
    public Cli WithPassword(string? password)
    {
        _password = password;
        return this;
    }

    [Description("Only supported on Windows")]
    public Cli WithPasswordInClearText(string passwordInClearText)
    {
        _passwordInClearText = passwordInClearText;
        return this;
    }

    public Cli WithStandardInputPipe(PipeSource source)
    {
        _standardInputPipe = source;
        return this;
    }

    public Cli WithStandardOutputPipe(PipeTarget target)
    {
        _standardOutputPipe = target;
        return this;
    }

    public Cli WithStandardErrorPipe(PipeTarget target)
    {
        _standardErrorPipe = target;
        return this;
    }

    public async Task<CliResult> ExecutePipeAsync(CancellationToken cancellationToken = default)
    {
        Cli current = this;
        Cli compositeCli = current;

        while (current.PipeTo != null)
        {
            compositeCli = compositeCli | current.PipeTo;
            current = current.PipeTo;
        }

        return await compositeCli.ExecuteAsync(cancellationToken);
    }

    public async Task<CliResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        await ExecuteAsync(cancellationToken, CancellationToken.None);

    public async Task<CliResult> ExecuteAsync(CancellationToken forcefulCancellationToken, CancellationToken gracefulCancellationToken)
    {
        ProcessEx process = new(CreateStartInfo());

        process.Start();
        return await ExecuteAsync(process, forcefulCancellationToken, gracefulCancellationToken);
    }

    private async Task<CliResult> ExecuteAsync(ProcessEx process, CancellationToken forcefulCancellationToken = default, CancellationToken gracefulCancellationToken = default)
    {
        using ProcessEx _ = process;
        using CancellationTokenSource waitTimeoutCts = new();
        using CancellationTokenRegistration _1 = forcefulCancellationToken.Register(() => waitTimeoutCts.CancelAfter(TimeSpan.FromSeconds(3)));
        using CancellationTokenSource stdInCts = CancellationTokenSource.CreateLinkedTokenSource(forcefulCancellationToken);

        using CancellationTokenRegistration _2 = forcefulCancellationToken.Register(process.Kill);
        using CancellationTokenRegistration _3 = gracefulCancellationToken.Register(process.Interrupt);

        Task pipingTask = Task.WhenAll(
            PipeStandardInputAsync(process, stdInCts.Token),
            PipeStandardOutputAsync(process, forcefulCancellationToken),
            PipeStandardErrorAsync(process, forcefulCancellationToken)
        );

        try
        {
            await process.WaitUntilExitAsync(waitTimeoutCts.Token).ConfigureAwait(false);

            stdInCts.Cancel();

            await pipingTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (
            ex.CancellationToken == forcefulCancellationToken
         || ex.CancellationToken == gracefulCancellationToken
         || ex.CancellationToken == waitTimeoutCts.Token
         || ex.CancellationToken == stdInCts.Token)
        {
        }

        if (forcefulCancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException($"Command execution canceled. Underlying process ({process.Name}#{process.Id}) was forcefully terminated.", forcefulCancellationToken);
        }

        if (gracefulCancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException($"Command execution canceled. Underlying process ({process.Name}#{process.Id}) was gracefully terminated.", gracefulCancellationToken);
        }

        return new CliResult(process.ExitCode, process.StartTime, process.ExitTime);
    }

    private async Task PipeStandardInputAsync(ProcessEx process, CancellationToken cancellationToken = default)
    {
        using (process.StandardInput)
        {
            try
            {
                await StandardInputPipe
                    .CopyToAsync(process.StandardInput, cancellationToken)
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (IOException ex) when (ex.GetType() == typeof(IOException))
            {
            }
        }
    }

    private async Task PipeStandardOutputAsync(ProcessEx process, CancellationToken cancellationToken = default)
    {
        using (process.StandardOutput)
        {
            await StandardOutputPipe
                .CopyFromAsync(process.StandardOutput, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task PipeStandardErrorAsync(ProcessEx process, CancellationToken cancellationToken = default)
    {
        using (process.StandardError)
        {
            await StandardErrorPipe
                .CopyFromAsync(process.StandardError, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private ProcessStartInfo CreateStartInfo()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = FileName,
            Arguments = Arguments,
            WorkingDirectory = WorkingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = UseShellExecute,
            CreateNoWindow = CreateNoWindow,
            Verb = Verb,
            WindowStyle = WindowStyle,
        };

        try
        {
            if (Domain is not null)
            {
                startInfo.Domain = Domain;
            }

            if (UserName is not null)
            {
                startInfo.UserName = UserName;
            }

            if (Password is not null)
            {
                startInfo.Password = Password.ToSecureString();
            }

            if (PasswordInClearText is not null)
            {
                startInfo.PasswordInClearText = PasswordInClearText;
            }

            if (LoadUserProfile)
            {
                startInfo.LoadUserProfile = LoadUserProfile;
            }

            if (ErrorDialog)
            {
                startInfo.ErrorDialog = ErrorDialog;
            }

            if (ErrorDialogParentHandle != IntPtr.Zero)
            {
                startInfo.ErrorDialogParentHandle = ErrorDialogParentHandle;
            }
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException(
                "Cannot start a process using the provided credentials. "
                    + "Setting custom domain, password, or loading user profile is only supported on Windows.",
                ex
            );
        }

        foreach (var pair in EnvironmentVariables)
        {
            var (key, value) = (pair.Key, pair.Value);

            if (value is not null)
            {
                startInfo.Environment[key] = value;
            }
            else
            {
                startInfo.Environment.Remove(key);
            }
        }

        return startInfo;
    }

    /// <summary>
    /// Only return the command line string
    /// </summary>
    /// <returns>cli</returns>
    public override string ToString()
    {
        if (PipeTo != null)
        {
            return $"{FileName.ToQuoteMarkArguments()} {Arguments} | {PipeTo}";
        }

        return $"{FileName.ToQuoteMarkArguments()} {Arguments}";
    }

    public static Cli operator |(Cli source, PipeTarget target)
        => source.WithStandardOutputPipe(target);

    public static Cli operator |(Cli source, Stream target)
        => source | PipeTarget.ToStream(target);

    public static Cli operator |(PipeSource source, Cli target) =>
        target.WithStandardInputPipe(source);

    public static Cli operator |(Stream source, Cli target) =>
        PipeSource.FromStream(source) | target;

    public static Cli operator |(Cli source, Cli target) =>
        PipeSource.FromCli(source) | target;
}

public interface ICli
{
    public string FileName { get; }

    public string Arguments { get; }

    public string WorkingDirectory { get; }

    public bool UseShellExecute { get; }

    public bool CreateNoWindow { get; }

    public Dictionary<string, string?> EnvironmentVariables { get; }

    public PipeSource StandardInputPipe { get; }

    public PipeTarget StandardOutputPipe { get; }

    public PipeTarget StandardErrorPipe { get; }
}
