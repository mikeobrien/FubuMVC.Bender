using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Bender;
using Bender.Extensions;
using FubuMVC.Bender;
using FubuMVC.Core.Http;
using FubuMVC.Core.Runtime;
using NSubstitute;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class XmlFormatterTests
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
            Deserialize<Model>(CreateXml("niels bohr")).String.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_xml_get()
        {
            Deserialize<Model>("", "GET").ShouldNotBeNull();
        }

        [Test]
        public void should_deserialize_xml_delete()
        {
            Deserialize<Model>("", "DELETE").ShouldNotBeNull();
        }

        [Test]
        public void should_serialize_xml()
        {
            var document = Serialize(new Model { String = "niels bohr" });
            document.Element("Model").Element("String").Value.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_xml_enum_string_value()
        {
            Deserialize<Model>(CreateXml(@enum: "Value2")).Enum.ShouldEqual(Enum.Value2);
        }

        [Test]
        public void should_serialize_xml_enum_string_value()
        {
            Serialize(new Model { Enum = Enum.Value2 }).Element("Model").Element("Enum").Value.ShouldEqual("Value2");
        }

        [Test]
        public void should_deserialize_xml_timespan_value()
        {
            Deserialize<Model>(CreateXml(timeSpan: "5.04:03:02.1")).TimeSpan.ShouldEqual(new TimeSpan(5, 4, 3, 2, 100));
        }

        [Test]
        public void should_serialize_xml_timespan_value()
        {
            Serialize(new Model { TimeSpan = new TimeSpan(5, 4, 3, 2, 100) }).Element("Model").Element("TimeSpan").Value.ShouldEqual("5.04:03:02.1000000");
        }

        [Test]
        public void should_serialize_xml_local_datetime_to_utc_iso8601_value()
        {
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            Serialize(new Model { DateTime = date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)) })
                .Element("Model").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_serialize_xml_utc_datetime_utc_iso8601_value()
        {
            Serialize(new Model { DateTime = new DateTime(2007, 4, 5, 23, 34, 59, DateTimeKind.Utc) })
                .Element("Model").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_deserialize_xml_local_iso8601_datetime_as_utc_to_local_value()
        {
            var result = Deserialize<Model>(CreateXml(dateTime: "2007-04-05T23:34:59")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        [Test]
        public void should_deserialize_xml_utc_iso8601_datetime_to_local_value()
        {
            var result = Deserialize<Model>(CreateXml(dateTime: "2007-04-05T23:34:59Z")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        private static T Deserialize<T>(string source, string method = "POST")
        {
            Debug.WriteLine(source);
            var inputStream = Substitute.For<IStreamingData>();
            var request = Substitute.For<ICurrentHttpRequest>();
            request.HttpMethod().Returns(method);
            inputStream.Input.Returns(source.ToAsciiStream());
            var deserializer = Deserializer.Create(x => x.Deserialization(y => 
                y.AddReader((v, s, t, o) => DateTime.Parse(s.Value.ToString()).ToLocalTime(), true)));
            return new XmlFormatter(inputStream, deserializer, null, null, request).Read<T>();
        }

        private static XDocument Serialize<T>(T model)
        {
            var outputWriter = Substitute.For<IOutputWriter>();
            var stream = new MemoryStream();
            outputWriter.WhenForAnyArgs(x => x.Write(null, y => { }))
                .Do(x => x.Arg<Action<Stream>>()(stream));
            var serializer = Serializer.Create(x => x.Serialization(y => 
                y.AddWriter<DateTime>((v, s, t, o) => v.ToUniversalTime().ToString("o"))));

            new XmlFormatter(null, null, outputWriter, serializer, 
                Substitute.For<ICurrentHttpRequest>()).Write(model, null);

            var result = stream.ReadAllText();
            Debug.WriteLine(result);
            return XDocument.Parse(result);
        }
    }
}
