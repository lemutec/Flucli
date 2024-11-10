using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Flucli.Utils.Extensions;

public static class ArgumentExtension
{
    /// <summary>
    /// Parses a command-line string into individual arguments, preserving quoted sections as single arguments.
    /// </summary>
    /// <param name="cli">The command-line string to parse.</param>
    /// <returns>An IEnumerable of parsed argument strings.</returns>
    public static IEnumerable<string> ToArguments(this string cli)
    {
        List<string> args = [];
        StringBuilder currentArg = new();
        bool inQuotes = false; // Tracks if currently inside quotes

        for (int i = 0; i < cli.Length; i++)
        {
            char c = cli[i];

            if (c == '"')
            {
                // Toggle quote state when encountering a quote character
                inQuotes = !inQuotes;

                // Append the quote to the current argument
                currentArg.Append(c);
            }
            else if (c == ' ' && !inQuotes)
            {
                // If a space is encountered outside of quotes, treat it as an argument separator
                if (currentArg.Length > 0)
                {
                    // Add the completed argument
                    args.Add(currentArg.ToString());

                    // Clear for the next argument
                    currentArg.Clear();
                }
            }
            else
            {
                // Append character to current argument
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
        {
            // Add the last argument if it exists
            args.Add(currentArg.ToString());
        }

        return args;
    }

    /// <summary>
    /// Joins a collection of arguments into a single command-line string, quoting arguments as needed.
    /// </summary>
    /// <param name="cli">The arguments to join into a command-line string.</param>
    /// <param name="option"></param>
    /// <returns>A single string representing the command-line.</returns>
    public static string ToArguments(this IEnumerable<string> cli, ArgumentBuilderOption? option = null)
    {
        option ??= ArgumentBuilderOption.Default;

        return string.Join(option.Separator, cli?.Select(arg => arg.ToArguments(option)) ?? []);
    }

    /// <summary>
    /// Adds quotes to an argument string if necessary, with options for handling existing quotes.
    /// </summary>
    /// <param name="arg">The argument string to process.</param>
    /// <param name="option"></param>
    /// <returns>The argument string, possibly with added quotes.</returns>
    public static string ToArguments(this string arg, ArgumentBuilderOption? option = null)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return arg;
        }

        option ??= ArgumentBuilderOption.Default;

        string quote = ArgumentBuilderOption.Quote;

        switch (option.QuoteReplace)
        {
            case QuoteReplace.DoubleQuote:
                arg = arg.Replace(ArgumentBuilderOption.Quote, ArgumentBuilderOption.DoubleQuote);
                quote = ArgumentBuilderOption.DoubleQuote;
                break;

            case QuoteReplace.BackSlashQuote:
                arg = arg.Replace(ArgumentBuilderOption.Quote, ArgumentBuilderOption.BackSlashQuote);
                quote = ArgumentBuilderOption.BackSlashQuote;
                break;
        }

        // If the argument is not already quoted
        // if the argument contains spaces
        // If the argument is a valid URI
        if ((!(arg.StartsWith(quote) && arg.EndsWith(quote)) && arg.Contains(' '))
           || (option.IsQuoteScheme && Uri.TryCreate(arg, UriKind.Absolute, out _)))
        {
            arg = quote + arg + quote;
        }
        return arg;
    }

    /// <summary>
    /// Splits a command-line string into a file name and its arguments.
    /// </summary>
    /// <param name="cli">The full command-line string.</param>
    /// <returns>A tuple containing the file name and an enumerable of arguments.</returns>
    public static (string, IEnumerable<string>) ToFileNameWithArguments(this string cli)
    {
        IEnumerable<string> args = cli.ToArguments();

        return args.Count() switch
        {
            0 => (cli, []),
            1 => (args.First(), []),
            _ => (args.First(), args.Skip(1)),
        };
    }

    /// <summary>
    /// Converts a plain string to a SecureString, which is a more secure way to handle sensitive information.
    /// </summary>
    /// <param name="str">The plain string to convert.</param>
    /// <returns>A SecureString containing the characters of the original string.</returns>
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
}
