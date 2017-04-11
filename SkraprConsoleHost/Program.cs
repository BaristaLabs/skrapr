namespace Skrapr
{
    using BaristaLabs.Skrapr;
    using BaristaLabs.Skrapr.Definitions;
    using EntryPoint;
    using Hangfire;
    using Hangfire.MemoryStorage;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.IO;

    class Program
    {
        //Launch chrome with
        //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223
        //Consider using PM2 to launch both chrome and SkraprCLI (is there a .net equiv?)

        static void Main(string[] args)
        {
            var cliArguments = Cli.Parse<CliArguments>(args);

            //Load and parse the Skrapr definition file to use.
            if (!File.Exists(cliArguments.SkraprDefinitionPath))
                throw new FileNotFoundException($"The specified skrapr definition ({cliArguments.SkraprDefinitionPath}) could not be found. Please check that the skrapr definition exists.");

            Console.WriteLine("Loading Skrapr Definition...");
            var skraprDefinitionJson = File.ReadAllText(cliArguments.SkraprDefinitionPath);
            var skraprDefinition = JsonConvert.DeserializeObject<SkraprDefinition>(skraprDefinitionJson);

            //Setup our DI
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            //Setup Hangfire
            GlobalConfiguration.Configuration.UseStorage(new MemoryStorage());

            Console.WriteLine("Connecting to a Chrome session...");

            var sessions = ChromeBrowser.GetChromeSessions(cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort).GetAwaiter().GetResult();
            var session = sessions.First(s => s.Type == "page");
            var devTools = SkraprDevTools.Connect(session).GetAwaiter().GetResult();
            Console.WriteLine($"Using session {session.Id}: {session.Title} - {session.WebSocketDebuggerUrl}");

            Console.WriteLine("Performing tasks...");
            devTools.Navigate("http://www.toririchard.com").GetAwaiter().GetResult();
            devTools.WaitForPageToStopLoading().GetAwaiter().GetResult();

            var currentUrl = devTools.EvaluateScript("window.location.toString()").GetAwaiter().GetResult();
            Console.WriteLine(currentUrl.Value);

            //using (new BackgroundJobServer())
            //{
            //    Console.WriteLine("Skrapr started. Press ENTER to exit...");

            //    Console.WriteLine("Executing initial Skrape...");
            //    var definitionJobId = BackgroundJob.Enqueue(() => Console.WriteLine("Foo"));


            //    //If the definition specifies a recurrence, specify the continue with it here
            //    //BackgroundJob.ContinueWith(definitionJobId, () => )
            //    RecurringJob.AddOrUpdate("1", () => Console.WriteLine("Foo"), () => "* * * * *");

            //    Console.ReadLine();
            //}

            Console.WriteLine("All Done!");
            Console.ReadLine();
        }
    }
}