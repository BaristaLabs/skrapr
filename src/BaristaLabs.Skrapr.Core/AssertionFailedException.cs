namespace BaristaLabs.Skrapr
{
    using System;

    /// <summary>
    /// Exception that is thrown when an Assertion task fails.
    /// </summary>
    [Serializable]
    public class AssertionFailedException : Exception
    {
        public AssertionFailedException()
        {
        }

        public AssertionFailedException(string message) : base(message)
        {
        }

        public AssertionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}