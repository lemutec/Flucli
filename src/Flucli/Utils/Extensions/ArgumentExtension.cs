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
        return string.Join(" ", cli?.Select(arg => (arg?.Contains(' ') ?? false) ? $"\"{arg}\"" : arg) ?? []);
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
}
