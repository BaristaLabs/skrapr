namespace BaristaLabs.Skrapr.Tasks
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Page = ChromeDevTools.Page;

    /// <summary>
    /// Represents a task that instructs the browser to go forward
    /// </summary>
    public class GoForwardTask : SkraprTask
    {
        public override string Name
        {
            get { return "GoForward"; }
        }

        /// <summary>
        /// Gets the number of steps to go back. Default: 1
        /// </summary>
        [DefaultValue(1)]
        public uint StepsForward
        {
            get;
            set;
        }

        public GoForwardTask()
        {
            StepsForward = 1;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var navigationHistory = await worker.Session.Page.GetNavigationHistory(new Page.GetNavigationHistoryCommand());

            var targetIndex = navigationHistory.CurrentIndex + StepsForward;
            if (targetIndex > navigationHistory.Entries.Length - 1)
                throw new InvalidOperationException($"Cannot go forward {StepsForward} steps, the navigation history doesn't go that far.");

            var targetNavigationEntry = navigationHistory.Entries[targetIndex];
            await worker.Session.Page.NavigateToHistoryEntry(new Page.NavigateToHistoryEntryCommand
            {
                EntryId = targetNavigationEntry.Id
            });
            await worker.DevTools.WaitForNextNavigation(cancellationToken: worker.CancellationToken);
        }
    }
}
