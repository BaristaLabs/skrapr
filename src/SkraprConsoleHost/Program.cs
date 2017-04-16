namespace Skrapr
{
    using BaristaLabs.Skrapr;
    using EntryPoint;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Json;
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

            //Remove previous log file
            File.Delete("skraprlog.json");

            //Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(new JsonFormatter(), "skraprlog.json")
                .WriteTo.ColoredConsole(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            //Configure the logger.
            var logger = serviceProvider
                .GetService<ILoggerFactory>()
                .AddSerilog()
                .CreateLogger<Program>();

            logger.LogDebug($"Connecting to a Chrome session on {cliArguments.RemoteDebuggingHost}:{cliArguments.RemoteDebuggingPort}...");

            var sessions = ChromeBrowser.GetChromeSessions(cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort).GetAwaiter().GetResult();
            var session = sessions.FirstOrDefault(s => s.Type == "page" && !String.IsNullOrWhiteSpace(s.WebSocketDebuggerUrl));

            //TODO: Create a new session if one doesn't exist.
            if (session == null)
                throw new InvalidOperationException("Unable to locate a suitable session -- ensure that the Developer Tools window is closed on an existing session.");

            var devTools = SkraprDevTools.Connect(serviceProvider, session).GetAwaiter().GetResult();
            logger.LogDebug($"Using session {session.Id}: {session.Title} - {session.WebSocketDebuggerUrl}");

            var processor = SkraprWorker.Create(cliArguments.SkraprDefinitionPath, devTools.Session, devTools);
            processor.AddStartUrls();

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

            logger.LogDebug("Processing...");
            Console.ReadLine();
        }
    }
}