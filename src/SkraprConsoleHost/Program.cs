namespace Skrapr
{
    using BaristaLabs.Skrapr;
    using EntryPoint;
    using Hangfire;
    using Hangfire.MemoryStorage;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Linq;

    class Program
    {
        //Launch chrome with
        //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223
        //Consider using PM2 to launch both chrome and SkraprCLI (is there a .net equiv?)

        static void Main(string[] args)
        {
            var cliArguments = Cli.Parse<CliArguments>(args);

            //Do an initial check to ensure that the Skrapr Definition exists.
            if (!File.Exists(cliArguments.SkraprDefinitionPath))
                throw new FileNotFoundException($"The specified skrapr definition ({cliArguments.SkraprDefinitionPath}) could not be found. Please check that the skrapr definition exists.");

            //Setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            Console.WriteLine("Connecting to a Chrome session...");

            var sessions = ChromeBrowser.GetChromeSessions(cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort).GetAwaiter().GetResult();
            var session = sessions.First(s => s.Type == "page");
            var devTools = SkraprDevTools.Connect(session).GetAwaiter().GetResult();
            Console.WriteLine($"Using session {session.Id}: {session.Title} - {session.WebSocketDebuggerUrl}");

            var processor = SkraprDefinitionProcessor.Create(cliArguments.SkraprDefinitionPath, devTools);
            processor.Begin();

            //Setup Hangfire
            //GlobalConfiguration.Configuration.UseStorage(new MemoryStorage());

            //using (new BackgroundJobServer())
            //{
            //    Console.WriteLine("Skrapr started. Press ENTER to exit...");

            //    Console.WriteLine("Executing initial Skrape...");
            //    var definitionJobId = BackgroundJob.Enqueue(() => SkraprDefinitionProcessor.Start(cliArguments.SkraprDefinitionPath, devTools));
            //    BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget"));
            //    Console.ReadKey();
            //}

            Console.WriteLine("All Done!");
            Console.ReadLine();
        }
    }
}