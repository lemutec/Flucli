using Flucli.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Flucli;

public static class CliExtensions
{
    /// <summary>
    /// Creates a new instance of the Cli class using the provided command-line string.
    /// </summary>
    /// <param name="cli">A command-line string to initialize the Cli instance with.</param>
    /// <returns>A new Cli instance initialized with the given command-line string.</returns>
    public static Cli CreateCli(this string cli)
    {
        return new Cli(cli);
    }

    /// <summary>
    /// Creates a new instance of the Cli class using a tuple containing a file name and arguments.
    /// </summary>
    /// <param name="cli">A tuple where the first item is the file name, and the second item is an enumerable of arguments.</param>
    /// <returns>A new Cli instance initialized with the given file name and arguments.</returns>
    public static Cli CreateCli(this (string FileName, IEnumerable<string> Arguments) cli)
    {
        return new Cli(cli.FileName).WithArguments(cli.Arguments);
    }

    /// <summary>
    /// Initializes a Cli instance with the specified command and its arguments.
    /// </summary>
    /// <param name="cli">The command to be executed.</param>
    /// <param name="arguments">A single string of arguments associated with the command.</param>
    /// <returns>A new Cli instance initialized with the given command and arguments.</returns>
    public static Cli WithArguments(this string cli, string arguments)
    {
        return new Cli(cli).WithArguments(arguments);
    }

    /// <summary>
    /// Parses a command-line string into a series of chained Cli instances,
    /// supporting commands separated by a pipe ('|') symbol.
    /// Note: Parsing commands with pipe symbols in program options is not supported.
    /// </summary>
    /// <param name="cli">The command-line string to parse, potentially containing pipe symbols.</param>
    /// <returns>
    /// A Cli instance representing the first command in the chain, with linked Cli instances for each subsequent piped command.
    /// </returns>
    public static Cli ParseCli(this string cli)
    {
        // The command string containing | is uncommon.
        // If necessary, please do not use this parse method.
        if (cli.Contains("|"))
        {
            IEnumerable<string> clis = cli.Split('|');
            IEnumerable<Cli> chain = clis.Select(cli => CreateCli(cli.ToFileNameWithArguments()));

            Cli header = chain.First();
            _ = chain.Skip(1).Aggregate(header, (prev, next) =>
            {
                prev.PipeTo = next;
                next.PipeFrom = prev;
                return next;
            });
            return header;
        }
        else
        {
            return CreateCli(cli.ToFileNameWithArguments());
        }
    }
}
