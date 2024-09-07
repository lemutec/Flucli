using System.Text;

namespace Flucli.Test;

internal class Program
{
    static void Main(string[] args)
    {
        Task.Run(async () =>
        {
            byte[] array = new byte[100];
            using MemoryStream ms = new(array);

            var command1 = "cmd"
                .WithArguments("/c echo Hello");

            var command2 = "cmd"
                .WithArguments("/c findstr H")
                .WithStandardOutputPipe(PipeTarget.ToStream(ms));

            CliResult result = await (command1 | command2).ExecuteAsync();
            string output = Encoding.UTF8.GetString(array);

            Console.WriteLine("STDOUT: " + output);
            Console.WriteLine("ExitCode is " + result.ExitCode);
        });

        Console.ReadLine();
    }
}
