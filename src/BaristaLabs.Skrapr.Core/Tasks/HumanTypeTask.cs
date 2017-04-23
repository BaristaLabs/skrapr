namespace BaristaLabs.Skrapr.Tasks
{
    using BaristaLabs.Skrapr.Extensions;
    using BaristaLabs.Skrapr.Utilities;
    using System.Threading.Tasks;
    using Dom = ChromeDevTools.DOM;
    using Input = ChromeDevTools.Input;

    /// <summary>
    /// Represents a task that types like a human; Slowly, with varing delay between keystrokes and making mistakes.
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

        public override async Task PerformTask(ISkraprWorker worker)
        {
            var keyEvents = InputUtils.ConvertInputToKeyEvents(Input);

            var nodeId = await worker.Session.DOM.GetNodeIdForSelector(Selector);

            await worker.Session.DOM.Focus(new Dom.FocusCommand
            {
                NodeId = nodeId
            }, worker.CancellationToken);

            foreach (var keyEvent in keyEvents)
            {
                await worker.Session.Input.DispatchKeyEvent(keyEvent, worker.CancellationToken);
                await Task.Delay(RandomUtils.Random.Next(5, 50));

                keyEvent.Type = "keyUp";
                await worker.Session.Input.DispatchKeyEvent(keyEvent, worker.CancellationToken);
                await Task.Delay(RandomUtils.Random.Next(50, 150));
            }
        }
    }
}
