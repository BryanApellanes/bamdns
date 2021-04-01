using System;
using Bam.Net.CommandLine;
using Bam.Net.Testing;
using DNS.Server;

namespace Bam.Net.Services
{
    [Serializable]
    public class ConsoleActions : CommandLineTool
    {
        private static SimpleDnsServer _server;

        [ConsoleAction("S", "Start the Dns server")]
        public static void StartServer()
        {
            _server = new SimpleDnsServer();
            _server.Start();
            Pause("bamdns started", () =>
            {
                OutLine("bamdns started", ConsoleColor.Green);
            });
        }

        [ConsoleAction("K", "Kill the Dns server")]
        public static void KillServer()
        {
            _server?.Stop();
            OutLine("bamdns stopped");
        }
        
    }
}