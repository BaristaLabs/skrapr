namespace BaristaLabs.Skrapr.Tasks
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Page = ChromeDevTools.Page;

    /// <summary>
    /// Represents a task that instructs the browser to go back
    /// </summary>
    public class GoBackTask : SkraprTask
    {
        public override string Name
        {
            get { return "GoBack"; }
        }

        /// <summary>
        /// Gets the number of steps to go back.
        /// </summary>
        [DefaultValue(1)]
        public int StepsBack
        {
            get;
            set;
        }

        public GoBackTask()
        {
            StepsBack = 1;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var navigationHistory = await worker.Session.Page.GetNavigationHistory(new Page.GetNavigationHistoryCommand());

            var targetIndex = navigationHistory.CurrentIndex - StepsBack;
            if (targetIndex < 0)
                throw new InvalidOperationException($"Cannot go back {StepsBack} steps, the navigation history doesn't go that far.");

            var targetNavigationEntry = navigationHistory.Entries[targetIndex];
            await worker.Session.Page.NavigateToHistoryEntry(new Page.NavigateToHistoryEntryCommand
            {
                EntryId = targetNavigationEntry.Id
            });
            await worker.DevTools.WaitForNextNavigation();
        }
    }
}
