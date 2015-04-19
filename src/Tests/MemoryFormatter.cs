using System;
using System.Collections.Generic;
using FubuMVC.Bender;

namespace Tests
{
    public class MemoryFormatter : IFormatter
    {
        private object _value;

        public MemoryFormatter()
        {
            MatchingMimetypes = new List<string>();
        }

        public IEnumerable<string> MatchingMimetypes { get; set; }
        public object WrittenMimeType { get; private set; }

        public T Read<T>()
        {
            return (T)_value;
        }

        public object Read(Type type)
        {
            return _value;
        }

        public void Write<T>(T target, string mimeType)
        {
            _value = target;
            WrittenMimeType = mimeType;
        }

        public void Write(object target, string mimeType)
        {
            _value = target;
            WrittenMimeType = mimeType;
        }
    }
}
