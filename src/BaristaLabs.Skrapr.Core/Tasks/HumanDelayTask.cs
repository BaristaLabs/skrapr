namespace BaristaLabs.Skrapr.Tasks
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using Troschuetz.Random;

    public class HumanDelayTask : SkraprTask
    {
        private static TRandom m_random = new TRandom();

        public override string Name
        {
            get { return "HumanDelay"; }
        }

        public int? MinDelay
        {
            get;
            set;
        }

        public int? MaxDelay
        {
            get;
            set;
        }

        public override async Task PerformTask(ISkraprWorker worker)
        {
            if (worker.IsDebugEnabled)
            {
                worker.Logger.LogDebug("{taskName} Skipping; currently in debug mode.", Name);
                return;
            }

            //For a random period of time, move the mouse around, scroll up and down, hover over anchor tags, etc.

            //Set defaults.
            if (MinDelay.HasValue == false)
                MinDelay = 5000;

            if (MaxDelay.HasValue == false)
                MaxDelay = 30000;

            if (MinDelay > MaxDelay)
                throw new InvalidOperationException($"MinDelay ({MinDelay}) must be less than MaxDelay ({MaxDelay})");

            var delay = m_random.Next(MinDelay.Value, MaxDelay.Value);

            worker.Logger.LogDebug("{taskName} delaying for {delay}ms", Name, delay);
            await Task.Delay(delay);

            //TODO: Improve this by scrolling and stuff.
        }
    }
}
