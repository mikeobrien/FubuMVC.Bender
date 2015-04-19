using System;
using System.IO;
using Bender;
using FubuCore.Descriptions;
using FubuMVC.Core.Http;
using FubuMVC.Core.Resources.Conneg;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Bender
{
    [MimeType(new[] { "text/json", "application/json" })]
    [Title("Json Serialization")]
    public class JsonFormatter : FormatterBase
    {
        private readonly Serializer _serializer;
        private readonly Deserializer _deserializer;

        public JsonFormatter(
            IStreamingData streaming,
            Deserializer deserializer,
            IOutputWriter writer,
            Serializer serializer,
            ICurrentHttpRequest request) :
            base(streaming, writer, request, "text/json", "application/json")
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public override void Serialize(object target, Stream stream)
        {
            _serializer.SerializeJsonStream(target, stream);
        }

        public override object Deserialize(Type type, Stream stream)
        {
            var json = new StreamReader(stream).ReadToEnd();
            try
            {
                return _deserializer.DeserializeJson(json, type);
            }
            catch (FriendlyBenderException exception)
            {
                throw new FriendlyDeserializationException(exception, json);
            }
            catch (Exception exception)
            {
                throw new DeserializationException(exception, json);
            }
        }
    }
}
