namespace Flucli;

public static class GeneratedExtensions
{
    public static Cli WithArguments(this string cli, string argument)
    {
        return new Cli(cli).WithArguments(argument);
    }
}
