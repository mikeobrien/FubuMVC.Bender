using System;
using System.Collections.Generic;
using System.IO;
using FubuMVC.Core.Http;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Bender
{
    public abstract class FormatterBase : IFormatter
    {
        public IEnumerable<string> MatchingMimetypes { get; private set; }

        private readonly IStreamingData _streaming;
        private readonly IOutputWriter _writer;
        private readonly ICurrentHttpRequest _request;

        protected FormatterBase(
            IStreamingData streaming,
            IOutputWriter writer,
            ICurrentHttpRequest request,
            params string[] mimeTypes)
        {
            _streaming = streaming;
            _writer = writer;
            _request = request;
            MatchingMimetypes = mimeTypes;
        }

        public abstract void Serialize(object target, Stream stream);
        public abstract object Deserialize(Type type, Stream stream);

        public void Write(object target, string mimeType)
        {
            _writer.Write(mimeType, stream => Serialize(target, stream));
        }

        public void Write<T>(T target, string mimeType)
        {
            Write((object)target, mimeType);
        }

        public object Read(Type type)
        {
            return _request.GetMethod() == HttpMethod.Post || _request.GetMethod() == HttpMethod.Put ?
                Deserialize(type, _streaming.Input) : Activator.CreateInstance(type);
        }

        public T Read<T>()
        {
            return (T)Read(typeof(T));
        }
    }
}
