namespace BaristaLabs.Skrapr.Tasks
{
    using System;
    using System.Threading.Tasks;
    using Input = ChromeDevTools.Input;

    /// <summary>
    /// Represents a task that types like a human; Slowly, with varing delay between keystrokes and making mistakes.
    /// </summary>

    /// <summary>
    /// Represents a task that adds the url of the page to the queue.
    /// </summary>
    public class HumanTypeTask : SkraprTask
    {
        public override string Name
        {
            get { return "HumanType"; }
        }

        /// <summary>
        /// Gets or sets the element that will be focused prior to sending the input (optional)
        /// </summary>
        public string Selector
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the text that will be entered
        /// </summary>
        /// <remarks>
        /// See https://autohotkey.com/docs/commands/Send.htm for an example of commands.
        /// </remarks>
        public string Input
        {
            get;
            set;
        }

        public override Task PerformTask(ISkraprWorker worker)
        {
            throw new NotImplementedException();
        }
    }
}
