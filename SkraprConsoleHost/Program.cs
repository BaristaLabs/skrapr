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

            Console.WriteLine("Launching Chrome Browser...");
            using (var chromeBrowser = ChromeBrowser.Launch())
            {
                var sessions = chromeBrowser.GetChromeSessions().GetAwaiter().GetResult();
                var devTools = SkraprDevTools.Connect(sessions.First(s => s.Type == "page")).GetAwaiter().GetResult();
                devTools.Navigate("http://www.toririchard.com").GetAwaiter().GetResult();
                devTools.WaitForPageToStopLoading().GetAwaiter().GetResult();

                var currentUrl = devTools.EvaluateScript("window.location.toString()").GetAwaiter().GetResult();
                Console.WriteLine(currentUrl);

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
            }
        }
    }
}