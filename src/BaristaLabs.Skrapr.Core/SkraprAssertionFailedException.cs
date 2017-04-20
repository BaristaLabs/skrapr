namespace BaristaLabs.Skrapr
{
    using System;

    /// <summary>
    /// Exception that is thrown when an Assertion task fails.
    /// </summary>
    [Serializable]
    public class SkraprAssertionFailedException : Exception
    {
        public SkraprAssertionFailedException()
        {
        }

        public SkraprAssertionFailedException(string message) : base(message)
        {
        }

        public SkraprAssertionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}