using System.Web.Routing;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Binding.InMemory;
using FubuCore.Descriptions;
using FubuMVC.Bender;
using FubuMVC.Core.Http;
using FubuMVC.Core.Http.Cookies;
using NSubstitute;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class BindingReaderTests
    {
        public class Model 
        {
            public string Value { get; set; }
        }

        private MemoryFormatter _formatter;
        private BindingReaderOptions _options;
        private RequestData _requestData;
        private BindingReader<Model, MemoryFormatter> _reader;

        [SetUp]
        public void Setup()
        {
            _reader = new BindingReader<Model, MemoryFormatter>(
                _formatter = new MemoryFormatter(), 
                _options = new BindingReaderOptions(),
                new ObjectResolver(new InMemoryServiceLocator(), 
                new BindingRegistry(), new NulloBindingLogger()), 
                _requestData = new RequestData(new RouteDataValues(new RouteData())),
                new InMemoryServiceLocator());
        }

        [Test]
        public void should_read()
        {
            var model = new Model();
            _formatter.Write(model, null);
            _reader.Read(null).ShouldEqual(model);
        }

        private void AddCookies()
        {
            var cookies = Substitute.For<ICookies>();
            cookies.Get("Value").Returns(new Cookie("Value", "hai"));
            _requestData.AddValues(new CookieValueSource(cookies)); 
        }

        [Test]
        public void should_not_bind_value_source()
        {
            var model = new Model();
            AddCookies();
            _formatter.Write(model, null);
            _reader.Read(null).Value.ShouldBeNull();
        }

        [Test]
        public void should_bind_value_source()
        {
            var model = new Model();
            AddCookies();
            _options.BindingSources.Add(RequestDataSource.Cookie);
            _formatter.Write(model, null);
            _reader.Read(null).Value.ShouldEqual("hai");
        }

        [Test]
        public void should_return_formatter_mime_types()
        {
            _reader.Mimetypes.ShouldEqual(_formatter.MatchingMimetypes);
        }

        [Test]
        public void should_set_description()
        {
            var description = new Description();
            _reader.Describe(description);
            description.Title.ShouldEqual(BindingReader
                .DescriptionFormat.ToFormat("MemoryFormatter"));
            description.Children[BindingReader.DescriptionChildKey]
                .Title.ShouldEqual("MemoryFormatter");
        }
    }
}
