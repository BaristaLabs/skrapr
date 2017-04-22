namespace BaristaLabs.Skrapr.Utilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains various console related utility methods
    /// </summary>
    public static class ConsoleUtils
    {
        /// <summary>
        /// Obtains the next character or function key pressed by the user
        /// asynchronously. The pressed key is displayed in the console window.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token that can be used to cancel the read.
        /// </param>
        /// <param name="responsiveness">
        /// The number of milliseconds to wait between polling the
        /// <see cref="Console.KeyAvailable"/> property.
        /// </param>
        /// <returns>Information describing what key was pressed.</returns>
        /// <exception cref="TaskCanceledException">
        /// Thrown when the read is cancelled by the user input (Ctrl+C etc.)
        /// or when cancellation is signalled via
        /// the passed <paramred name="cancellationToken"/>.
        /// </exception>
        public static async Task ReadKeyAsync(
            ConsoleKey keyToWaitFor,
            CancellationToken cancellationToken,
            int responsiveness = 100)
        {
            var cancelPressed = false;
            var cancelWatcher = new ConsoleCancelEventHandler(
                (sender, args) => { cancelPressed = true; });
            Console.CancelKeyPress += cancelWatcher;
            try
            {
                while (!cancelPressed && !cancellationToken.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        if (Console.ReadKey().Key == keyToWaitFor)
                            return;
                    }

                    await Task.Delay(
                        responsiveness,
                        cancellationToken);
                }

                if (cancelPressed)
                {
                    throw new TaskCanceledException("Cancelled by user input.");
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                Console.CancelKeyPress -= cancelWatcher;
            }
        }
    }
}
