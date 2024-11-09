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
    /// <returns>A single string representing the command-line.</returns>
    public static string ToArguments(this IEnumerable<string> cli)
    {
        return string.Join(" ", cli?.Select(arg => arg.ToQuoteMarkArguments()) ?? []);
    }

    /// <summary>
    /// Adds quotes to an argument string if necessary, with options for handling existing quotes.
    /// </summary>
    /// <param name="arg">The argument string to process.</param>
    /// <param name="quoteType">Specifies how to handle existing quotes within the argument.</param>
    /// <returns>The argument string, possibly with added quotes.</returns>
    public static string ToQuoteMarkArguments(this string arg, QuoteReplace quoteType = QuoteReplace.None)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return arg;
        }

        switch (quoteType)
        {
            case QuoteReplace.DoubleQuote:
                arg = arg.Replace("\"", "\"\"");
                break;

            case QuoteReplace.BackSlashQuote:
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

    /// <summary>
    /// Splits a command-line string into a file name and its arguments.
    /// </summary>
    /// <param name="cli">The full command-line string.</param>
    /// <returns>A tuple containing the file name and an enumerable of arguments.</returns>
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

    /// <summary>
    /// Options for handling quotes within argument strings.
    /// </summary>
    public enum QuoteReplace
    {
        /// <summary>
        /// No special handling for quotes
        /// </summary>
        None,

        /// <summary>
        /// Replace each quote with two quotes
        /// </summary>
        DoubleQuote,

        /// <summary>
        /// Escape each quote with a backslash
        /// </summary>
        BackSlashQuote,
    }
}
