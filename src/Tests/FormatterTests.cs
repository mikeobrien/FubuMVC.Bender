using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bender;
using FubuCore;
using FubuMVC.Bender;
using FubuMVC.Core.Http;
using FubuMVC.Core.Runtime;
using NSubstitute;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class FormatterTests
    {
        public enum Enum { Value1, Value2 }

        public class Model
        {
            public string String { get; set; }
            public DateTime DateTime { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public Enum Enum { get; set; }
            public Guid Guid { get; set; }
            public int Int { get; set; }
            public double Double { get; set; }
        }

        // Json

        private static string CreateJson(string @string = null, string dateTime = null, string timeSpan = null,
                                 string @enum = null, string guid = null, string @int = null, string @double = null)
        {
            var json = "{";
            if (@string.IsNotNull()) json += "\"String\": \"{0}\", ".ToFormat(@string);
            if (@enum.IsNotNull()) json += "\"Enum\": \"{0}\", ".ToFormat(@enum);
            if (dateTime.IsNotNull()) json += "\"DateTime\": \"{0}\", ".ToFormat(dateTime);
            if (timeSpan.IsNotNull()) json += "\"TimeSpan\": \"{0}\", ".ToFormat(timeSpan);
            if (guid.IsNotNull()) json += "\"Guid\": \"{0}\", ".ToFormat(guid);
            if (@int.IsNotNull()) json += "\"Int\": {0},".ToFormat(@int);
            if (@double.IsNotNull()) json += "\"Double\": {0}".ToFormat(@double);
            return json + "}";
        }

        [Test]
        public void should_deserialize_json()
        {
            DeserializeJson<Model>(CreateJson("niels bohr")).String.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_json_get()
        {
            DeserializeJson<Model>("", "GET").ShouldNotBeNull();
        }

        [Test]
        public void should_deserialize_json_delete()
        {
            DeserializeJson<Model>("", "DELETE").ShouldNotBeNull();
        }

        [Test]
        public void should_serialize_json()
        {
            var document = SerializeJson(new Model { String = "niels bohr" });
            document.Element("root").Element("String").Value.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_json_enum_string_value()
        {
            DeserializeJson<Model>(CreateJson(@enum: "Value2")).Enum.ShouldEqual(Enum.Value2);
        }

        [Test]
        public void should_serialize_json_enum_string_value()
        {
            SerializeJson(new Model { Enum = Enum.Value2 }).Element("root").Element("Enum").Value.ShouldEqual("Value2");
        }

        [Test]
        public void should_deserialize_json_timespan_value()
        {
            DeserializeJson<Model>(CreateJson(timeSpan: "5.04:03:02.1")).TimeSpan.ShouldEqual(new TimeSpan(5, 4, 3, 2, 100));
        }

        [Test]
        public void should_serialize_json_timespan_value()
        {
            SerializeJson(new Model { TimeSpan = new TimeSpan(5, 4, 3, 2, 100) }).Element("root").Element("TimeSpan").Value.ShouldEqual("5.04:03:02.1000000");
        }

        [Test]
        public void should_serialize_json_local_datetime_to_utc_iso8601_value()
        {
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            SerializeJson(new Model { DateTime = date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)) })
                .Element("root").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_serialize_json_utc_datetime_utc_iso8601_value()
        {
            SerializeJson(new Model { DateTime = new DateTime(2007, 4, 5, 23, 34, 59, DateTimeKind.Utc) })
                .Element("root").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_deserialize_json_local_iso8601_datetime_as_utc_to_local_value()
        {
            var result = DeserializeJson<Model>(CreateJson(dateTime: "2007-04-05T23:34:59")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        [Test]
        public void should_deserialize_json_utc_iso8601_datetime_to_local_value()
        {
            var result = DeserializeJson<Model>(CreateJson(dateTime: "2007-04-05T23:34:59Z")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        // Xml

        private static string CreateXml(string @string = null, string dateTime = null, string timeSpan = null,
                                 string @enum = null, string guid = null, string @int = null, string @double = null)
        {
            var xml = "<Model>";
            if (@string.IsNotNull()) xml += "<String>{0}</String>".ToFormat(@string);
            if (@enum.IsNotNull()) xml += "<Enum>{0}</Enum>".ToFormat(@enum);
            if (dateTime.IsNotNull()) xml += "<DateTime>{0}</DateTime>".ToFormat(dateTime);
            if (timeSpan.IsNotNull()) xml += "<TimeSpan>{0}</TimeSpan>".ToFormat(timeSpan);
            if (guid.IsNotNull()) xml += "<Guid>{0}</Guid>".ToFormat(guid);
            if (@int.IsNotNull()) xml += "<Int>{0}</Int>".ToFormat(@int);
            if (@double.IsNotNull()) xml += "<Double>{0}</Double>".ToFormat(@double);
            return xml + "</Model>";
        }

        [Test]
        public void should_deserialize_xml()
        {
            DeserializeXml<Model>(CreateXml("niels bohr")).String.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_xml_get()
        {
            DeserializeXml<Model>("", "GET").ShouldNotBeNull();
        }

        [Test]
        public void should_deserialize_xml_delete()
        {
            DeserializeXml<Model>("", "DELETE").ShouldNotBeNull();
        }

        [Test]
        public void should_serialize_xml()
        {
            var document = SerializeXml(new Model { String = "niels bohr" });
            document.Element("Model").Element("String").Value.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_xml_enum_string_value()
        {
            DeserializeXml<Model>(CreateXml(@enum: "Value2")).Enum.ShouldEqual(Enum.Value2);
        }

        [Test]
        public void should_serialize_xml_enum_string_value()
        {
            SerializeXml(new Model { Enum = Enum.Value2 }).Element("Model").Element("Enum").Value.ShouldEqual("Value2");
        }

        [Test]
        public void should_deserialize_xml_timespan_value()
        {
            DeserializeXml<Model>(CreateXml(timeSpan: "5.04:03:02.1")).TimeSpan.ShouldEqual(new TimeSpan(5, 4, 3, 2, 100));
        }

        [Test]
        public void should_serialize_xml_timespan_value()
        {
            SerializeXml(new Model { TimeSpan = new TimeSpan(5, 4, 3, 2, 100) }).Element("Model").Element("TimeSpan").Value.ShouldEqual("5.04:03:02.1000000");
        }

        [Test]
        public void should_serialize_xml_local_datetime_to_utc_iso8601_value()
        {
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            SerializeXml(new Model { DateTime = date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)) })
                .Element("Model").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_serialize_xml_utc_datetime_utc_iso8601_value()
        {
            SerializeXml(new Model { DateTime = new DateTime(2007, 4, 5, 23, 34, 59, DateTimeKind.Utc) })
                .Element("Model").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_deserialize_xml_local_iso8601_datetime_as_utc_to_local_value()
        {
            var result = DeserializeXml<Model>(CreateXml(dateTime: "2007-04-05T23:34:59")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        [Test]
        public void should_deserialize_xml_utc_iso8601_datetime_to_local_value()
        {
            var result = DeserializeXml<Model>(CreateXml(dateTime: "2007-04-05T23:34:59Z")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        private static T DeserializeJson<T>(string json, string method = "POST")
        {
            return Deserialize<T>(json, (i, d, r) => new JsonFormatter(i, d, null, null, r), method);
        }

        private static T DeserializeXml<T>(string xml, string method = "POST")
        {
            return Deserialize<T>(xml, (i, d, r) => new XmlFormatter(i, d, null, null, r), method);
        }

        private static T Deserialize<T>(string source, Func<IStreamingData, Deserializer, ICurrentHttpRequest, IFormatter> formatter, string method = "POST")
        {
            Debug.WriteLine(source);
            var inputStream = Substitute.For<IStreamingData>();
            var request = Substitute.For<ICurrentHttpRequest>();
            request.HttpMethod().Returns(method);
            inputStream.Input.Returns(source.ToAsciiStream());
            var deserializer = Deserializer.Create(x => x.Deserialization(y => y.AddReader((v, s, t, o) => DateTime.Parse(s.Value.ToString()).ToLocalTime(), true)));
            return formatter(inputStream, deserializer, request).Read<T>();
        }

        private static XDocument SerializeJson<T>(T model)
        {
            return Serialize(model, (o, s) => new JsonFormatter(null, null, o, s, Substitute.For<ICurrentHttpRequest>()),
                x => XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(new MemoryStream(Encoding.UTF8.GetBytes(x)), new XmlDictionaryReaderQuotas())));
        }

        private static XDocument SerializeXml<T>(T model)
        {
            return Serialize(model, (o, s) => new XmlFormatter(null, null, o, s, Substitute.For<ICurrentHttpRequest>()), XDocument.Parse);
        }

        private static XDocument Serialize<T>(T model, Func<IOutputWriter, Serializer, IFormatter> formatter, Func<string, XDocument> load)
        {
            var outputWriter = Substitute.For<IOutputWriter>();
            var stream = new MemoryStream();
            outputWriter.WhenForAnyArgs(x => x.Write(null, y => { }))
                .Do(x => x.Arg<Action<Stream>>()(stream));
            var serializer = Serializer.Create(x => x.Serialization(y => y.AddWriter<DateTime>((v, s, t, o) => v.ToUniversalTime().ToString("o"))));

            formatter(outputWriter, serializer).Write(model, null);

            var result = stream.ReadAllText();
            Debug.WriteLine(result);
            return load(result);
        }
    }
}
