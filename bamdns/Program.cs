using System;
using Bam.Net.CommandLine;
using Bam.Net.Services;
using Bam.Net.Testing;

namespace Bam.Net.System
{
    [Serializable]
    public class Program : CommandLineTool
    {
        static void Main(string[] args)
        {
            TryWritePid(true);
            IsolateMethodCalls = false;
            AddSwitches(typeof(ConsoleActions));
            AddConfigurationSwitches();
            Initialize(args, (a) =>
            {
                Message.PrintLine("Error parsing arguments: {0}", ConsoleColor.Red, a.Message);
                Environment.Exit(1);
            });

            if (!ExecuteSwitches(Arguments, new ConsoleActions()))
            {
                Interactive();
            }
        }
    }
}
