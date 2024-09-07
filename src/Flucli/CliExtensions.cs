namespace Flucli;

public static class CliExtensions
{
    public static Cli CreateCli(this string cli)
    {
        return new Cli(cli);
    }

    public static Cli WithArguments(this string cli, string arguments)
    {
        return new Cli(cli).WithArguments(arguments);
    }
}
