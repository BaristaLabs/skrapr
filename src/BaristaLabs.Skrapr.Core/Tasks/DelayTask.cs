namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Utilities;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class DelayTask : SkraprTask
    {
        public override string Name
        {
            get { return "Delay"; }
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
            //Set defaults.
            if (MinDelay.HasValue == false)
                MinDelay = 5000;

            if (MaxDelay.HasValue == false)
                MaxDelay = 10000;

            if (MinDelay > MaxDelay)
                throw new InvalidOperationException($"MinDelay ({MinDelay}) must be less than MaxDelay ({MaxDelay})");

            var delay = RandomUtils.Random.Next(MinDelay.Value, MaxDelay.Value);

            worker.Logger.LogDebug("{taskName} delaying for {delay}ms", Name, delay);
            await Task.Delay(delay, worker.CancellationToken);
        }
    }
}
