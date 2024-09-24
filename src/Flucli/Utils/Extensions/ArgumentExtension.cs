using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Flucli.Utils.Extensions;

public static class ArgumentExtension
{
    public static IEnumerable<string> ToArguments(this string cli)
    {
        List<string> args = [];
        string currentArg = string.Empty;
        bool inQuotes = false;

        for (int i = 0; i < cli.Length; i++)
        {
            char c = cli[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (currentArg != string.Empty)
                {
                    args.Add(currentArg);
                    currentArg = string.Empty;
                }
            }
            else
            {
                currentArg += c;
            }
        }

        if (currentArg != string.Empty)
        {
            args.Add(currentArg);
        }

        return args;
    }

    public static string ToArguments(this IEnumerable<string> cli)
    {
        return string.Join(" ", cli?.Select(arg => arg.ToQuoteMarkArguments()) ?? []);
    }

    public static string ToQuoteMarkArguments(this string arg, QuoteRepalce quoteType = QuoteRepalce.None)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return arg;
        }

        switch (quoteType)
        {
            case QuoteRepalce.DoubleQuote:
                arg = arg.Replace("\"", "\"\"");
                break;

            case QuoteRepalce.BackSlashQuote:
                arg = arg.Replace("\"", "\\\"");
                break;
        }

        if (!(arg.StartsWith("\"") && arg.EndsWith("\"")) // If the argument is not already quoted
           && arg.Contains(' ') || arg.Contains('\"'))    // And if the argument contains spaces or quotes
        {
            arg = $"\"{arg}\"";
        }
        return arg;
    }

    public static (string, IEnumerable<string>) ToFileNameWithArguments(this string cli)
    {
        IEnumerable<string> args = cli.ToArguments();

        if (args.Count() == 0)
        {
            return (cli, []);
        }
        else if (args.Count() == 1)
        {
            return (args.First(), []);
        }
        else
        {
            return (args.First(), args.Skip(1));
        }
    }

    public static SecureString ToSecureString(this string str)
    {
        SecureString secureString = new();

        foreach (char c in str)
        {
            secureString.AppendChar(c);
        }
        secureString.MakeReadOnly();
        return secureString;
    }

    public enum QuoteRepalce
    {
        None,
        DoubleQuote,
        BackSlashQuote,
    }
}
