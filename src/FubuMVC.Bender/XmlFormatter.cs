using System;
using System.IO;
using Bender;
using FubuCore.Descriptions;
using FubuMVC.Core.Http;
using FubuMVC.Core.Resources.Conneg;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Bender
{
    [MimeType(new[] { "text/xml", "application/xml" })]
    [Title("Xml Serialization")]
    public class XmlFormatter : FormatterBase
    {
        private readonly Serializer _serializer;
        private readonly Deserializer _deserializer;

        public XmlFormatter(
            IStreamingData streaming,
            Deserializer deserializer,
            IOutputWriter writer,
            Serializer serializer,
            ICurrentHttpRequest request) :
            base(streaming, writer, request, "text/xml", "application/xml")
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        protected override void Serialize(object target, Stream stream)
        {
            _serializer.SerializeXmlStream(target, stream);
        }

        protected override object Deserialize(Type type, Stream stream)
        {
            var xml = new StreamReader(stream).ReadToEnd();
            try
            {
                return _deserializer.DeserializeXml(xml, type);
            }
            catch (FriendlyBenderException exception)
            {
                throw new FriendlyDeserializationException(exception, xml);
            }
            catch (Exception exception)
            {
                throw new DeserializationException(exception, xml);
            }
        }
    }
}
