using System.Text;

namespace Flucli.Test;

internal class Program
{
    static void Main(string[] args)
    {
        Task.Run(async () =>
        {
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
        });

        Console.ReadLine();
    }
}
