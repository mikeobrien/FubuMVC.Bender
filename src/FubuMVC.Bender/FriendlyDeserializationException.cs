using System;
using Bender;

namespace FubuMVC.Bender
{
    public class FriendlyDeserializationException : Exception
    {
        public FriendlyDeserializationException(FriendlyBenderException exception, string source) :
            base(exception.Message, exception)
        {
            FriendlyMessage = exception.FriendlyMessage;
            RequestData = source;
        }

        public string FriendlyMessage { get; private set; }
        public string RequestData { get; private set; }
    }
}
