using System;

namespace FubuMVC.Bender
{
    public class DeserializationException : Exception
    {
        public DeserializationException(Exception exception, string source) :
            base(exception.Message, exception)
        {
            RequestData = source;
        }

        public string RequestData { get; private set; }
    }
}
