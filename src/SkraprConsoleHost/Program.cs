namespace Skrapr
{
    using BaristaLabs.Skrapr;
    using BaristaLabs.Skrapr.Extensions;
    using BaristaLabs.Skrapr.Tasks;
    using BaristaLabs.Skrapr.Utilities;
    using EntryPoint;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        //Launch chrome with
        //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223

        static int Main(string[] args)
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

            ChromeBrowser browser = null;
            SkraprDevTools devTools = null;
            SkraprWorker worker = null;

            try
            {
                if (cliArguments.Launch)
                {
                    browser = ChromeBrowser.Launch(cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort);
                }

                logger.LogInformation("Connecting to a Chrome session on {chromeHost}:{chromeRemoteDebuggingPort}...", cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort);

                ChromeSessionInfo session = null;
                try
                {
                    var sessions = ChromeBrowser.GetChromeSessions(cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort).GetAwaiter().GetResult();
                    session = sessions.FirstOrDefault(s => s.Type == "page" && !String.IsNullOrWhiteSpace(s.WebSocketDebuggerUrl));
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    logger.LogWarning("Unable to connect to a Chrome session on {chromeHost}:{chromeRemoteDebuggingPort}.", cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort);
                    logger.LogWarning("Please ensure that a chrome session has been launched with the --remote-debugging-port={chromeRemoteDebuggingPort} command line argument", cliArguments.RemoteDebuggingPort);
                    logger.LogWarning("Or, launch SkraprConsoleHost with -l");
                    return -1;
                }

                //TODO: Create a new session if one doesn't exist.
                if (session == null)
                {
                    logger.LogWarning("Unable to locate a suitable session. Ensure that the Developer Tools window is closed on an existing session or create a new chrome instance with the --remote-debugging-port={chromeRemoteDebuggingPort) command line argument", cliArguments.RemoteDebuggingPort);
                    return -1;
                }

                devTools = SkraprDevTools.Connect(serviceProvider, session).GetAwaiter().GetResult();
                logger.LogInformation("Using session {sessionId}: {sessionTitle} - {webSocketDebuggerUrl}", session.Id, session.Title, session.WebSocketDebuggerUrl);

                worker = SkraprWorker.Create(serviceProvider, cliArguments.SkraprDefinitionPath, devTools.Session, devTools, debugMode: cliArguments.Debug);

                if (cliArguments.Debug)
                {
                    logger.LogInformation($"Operating in debug mode. Tasks may perform additional behavior or may skip themselves.");
                }

                if (cliArguments.Attach == true)
                {
                    var targetInfo = devTools.Session.Target.GetTargetInfo(session.Id).GetAwaiter().GetResult();
                    var matchingRuleCount = worker.GetMatchingRules().GetAwaiter().GetResult().Count();
                    if (matchingRuleCount > 0)
                    {
                        logger.LogInformation($"Attach specified and {matchingRuleCount} rules match the current session's state; Continuing.", matchingRuleCount);
                        worker.Post(new NavigateTask
                        {
                            Url = targetInfo.Url
                        });
                    }
                    else
                    {
                        logger.LogInformation($"Attach specified but no rules matched the current session's state; Adding start tasks.");
                        worker.AddStartUrls();
                    }
                }
                else
                {
                    logger.LogInformation($"Adding start tasks.");
                    worker.AddStartUrls();
                }

                Console.TreatControlCAsInput = true;
                logger.LogInformation("Skrapr is currently processing. Press ENTER to exit...");

                var cancelKeyTokenSource = new CancellationTokenSource();

                var workerCompletion = worker.Completion
                    .ContinueWith((t) => cancelKeyTokenSource.Cancel());

                var keyCompletion = ConsoleUtils.ReadKeyAsync(ConsoleKey.Enter, cancelKeyTokenSource.Token)
                    .ContinueWith(async (t) =>
                    {
                        if (!t.IsCanceled)
                        {
                            logger.LogWarning("Stop requested at the console, cancelling...");
                            worker.Cancel();
                            await worker.Completion;
                        }

                    });

                Task.WaitAny(workerCompletion, keyCompletion);

                if (worker.Completion.IsFaulted)
                {
                    logger.LogError("Worker was faulted. Exiting with status code of -1");
                    return -1;
                }
            }
            catch(TaskCanceledException)
            {
                //Do Nothing
            }
            finally
            {
                //Cleanup.
                if (worker != null)
                {
                    worker.Dispose();
                    worker = null;
                }

                if (devTools != null)
                {
                    devTools.Dispose();
                    devTools = null;
                }

                if (browser != null)
                {
                    browser.Dispose();
                    browser = null;
                }
            }

            //Debugger.Break();
            logger.LogInformation("Worker completed successfully. Status code 0");
            return 0;
        }
    }
}