namespace BaristaLabs.Skrapr
{
    using System;
    using Runtime = ChromeDevTools.Runtime;

    public class JavaScriptException : Exception
    {
        private readonly Runtime.ExceptionDetails m_exceptionObject;

        public JavaScriptException(Runtime.ExceptionDetails jsException)
            :base(jsException.Text)
        {
            m_exceptionObject = jsException;
        }

        public Runtime.ExceptionDetails ExceptionDetails
        {
            get { return m_exceptionObject; }
        }
    }
}
