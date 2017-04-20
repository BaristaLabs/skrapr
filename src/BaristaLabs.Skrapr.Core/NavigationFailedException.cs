namespace BaristaLabs.Skrapr
{
    using System;

    /// <summary>
    /// Exception that is thrown when an expected navigation fails.
    /// </summary>
    [Serializable]
    public class NavigationFailedException : Exception
    {
        public NavigationFailedException()
        {
        }

        public NavigationFailedException(string message) : base(message)
        {
        }

        public NavigationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
