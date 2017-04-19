namespace BaristaLabs.Skrapr
{
    using Page = ChromeDevTools.Page;

    /// <summary>
    /// Encapsulates the current state of a Skrapr frame.
    /// </summary>
    public class SkraprFrameState
    {
        /// <summary>
        /// Gets or sets the title of the current frame's page.
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets information about the current frame's resources.
        /// </summary>
        public Page.FrameResourceTree FrameTree
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the url of the location where the current frame is at.
        /// </summary>
        public string Url
        {
            get;
            set;
        }
    }
}
