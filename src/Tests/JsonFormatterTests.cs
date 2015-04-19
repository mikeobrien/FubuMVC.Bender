using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
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
    public class JsonFormatterTests
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
            Deserialize<Model>(CreateJson("niels bohr")).String.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_json_get()
        {
            Deserialize<Model>("", "GET").ShouldNotBeNull();
        }

        [Test]
        public void should_deserialize_json_delete()
        {
            Deserialize<Model>("", "DELETE").ShouldNotBeNull();
        }

        [Test]
        public void should_serialize_json()
        {
            var document = Serialize(new Model { String = "niels bohr" });
            document.Element("root").Element("String").Value.ShouldEqual("niels bohr");
        }

        [Test]
        public void should_deserialize_json_enum_string_value()
        {
            Deserialize<Model>(CreateJson(@enum: "Value2")).Enum.ShouldEqual(Enum.Value2);
        }

        [Test]
        public void should_serialize_json_enum_string_value()
        {
            Serialize(new Model { Enum = Enum.Value2 }).Element("root")
                .Element("Enum").Value.ShouldEqual("Value2");
        }

        [Test]
        public void should_deserialize_json_timespan_value()
        {
            Deserialize<Model>(CreateJson(timeSpan: "5.04:03:02.1"))
                .TimeSpan.ShouldEqual(new TimeSpan(5, 4, 3, 2, 100));
        }

        [Test]
        public void should_serialize_json_timespan_value()
        {
            Serialize(new Model { TimeSpan = new TimeSpan(5, 4, 3, 2, 100) })
                .Element("root").Element("TimeSpan").Value.ShouldEqual("5.04:03:02.1000000");
        }

        [Test]
        public void should_serialize_json_local_datetime_to_utc_iso8601_value()
        {
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            Serialize(new Model { DateTime = date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)) })
                .Element("root").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_serialize_json_utc_datetime_utc_iso8601_value()
        {
            Serialize(new Model { DateTime = new DateTime(2007, 4, 5, 23, 34, 59, DateTimeKind.Utc) })
                .Element("root").Element("DateTime").Value.ShouldEqual("2007-04-05T23:34:59.0000000Z");
        }

        [Test]
        public void should_deserialize_json_local_iso8601_datetime_as_utc_to_local_value()
        {
            var result = Deserialize<Model>(CreateJson(dateTime: "2007-04-05T23:34:59")).DateTime;
            result.Kind.ShouldEqual(DateTimeKind.Local);
            var date = new DateTime(2007, 4, 5, 23, 34, 59);
            result.ShouldEqual(date.Add(TimeZone.CurrentTimeZone.GetUtcOffset(date)));
        }

        [Test]
        public void should_deserialize_json_utc_iso8601_datetime_to_local_value()
        {
            var result = Deserialize<Model>(CreateJson(dateTime: "2007-04-05T23:34:59Z")).DateTime;
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
            return new JsonFormatter(inputStream, deserializer, null, null, request).Read<T>();
        }

        private static XDocument Serialize<T>(T model)
        {
            var outputWriter = Substitute.For<IOutputWriter>();
            var stream = new MemoryStream();
            outputWriter.WhenForAnyArgs(x => x.Write(null, y => { }))
                .Do(x => x.Arg<Action<Stream>>()(stream));
            var serializer = Serializer.Create(x => x.Serialization(y => 
                y.AddWriter<DateTime>((v, s, t, o) => v.ToUniversalTime().ToString("o"))));

            new JsonFormatter(null, null, outputWriter, serializer, 
                Substitute.For<ICurrentHttpRequest>()).Write(model, null);

            var result = stream.ReadAllText();
            Debug.WriteLine(result);

            return XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(
                new MemoryStream(Encoding.UTF8.GetBytes(result)), 
                new XmlDictionaryReaderQuotas()));
        }
    }
}
