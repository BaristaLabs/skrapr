namespace BaristaLabs.Skrapr
{
    using BaristaLabs.Skrapr.ChromeDevTools;
    using BaristaLabs.Skrapr.Definitions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents a worker that processes a skrapr definition.
    /// </summary>
    public interface ISkraprWorker
    {
        /// <summary>
        /// Gets the definition that the worker works on.
        /// </summary>
        SkraprDefinition Definition
        {
            get;
        }

        /// <summary>
        /// Gets the Skrapr dev tools associated with the worker
        /// </summary>
        SkraprDevTools DevTools
        {
            get;
        }

        /// <summary>
        /// Gets a logger associated with the worker.
        /// </summary>
        ILogger Logger
        {
            get;
        }

        /// <summary>
        /// Gets the chrome session associated with the worker
        /// </summary>
        ChromeSession Session
        {
            get;
        }

        /// <summary>
        /// Adds a url to the urls being processed by the worker.
        /// </summary>
        /// <param name="url"></param>
        void AddUrl(string url);
    }
}
