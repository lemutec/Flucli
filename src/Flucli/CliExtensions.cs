using Flucli.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Flucli;

public static class CliExtensions
{
    public static Cli CreateCli(this string cli)
    {
        return new Cli(cli);
    }

    public static Cli CreateCli(this (string FileName, IEnumerable<string> Arguments) cli)
    {
        return new Cli(cli.FileName).WithArguments(cli.Arguments);
    }

    public static Cli WithArguments(this string cli, string arguments)
    {
        return new Cli(cli).WithArguments(arguments);
    }

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
