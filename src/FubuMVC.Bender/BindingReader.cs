using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Binding.InMemory;
using FubuCore.Descriptions;
using FubuMVC.Core.Http;
using FubuMVC.Core.Resources.Conneg;

namespace FubuMVC.Bender
{
    public class BindingReaderOptions
    {
        public BindingReaderOptions()
        {
            BindingSources = new List<RequestDataSource> { RequestDataSource.Route };
        }

        public List<RequestDataSource> BindingSources { get; private set; }
    }

    public class BindingReader
    {
        public const string DescriptionFormat = "Reading with {0}.";
        public const string DescriptionChildKey = "Formatter";
    }

    public class BindingReader<T, TFormatter> : BindingReader, 
        IReader<T>, DescribesItself where TFormatter : IFormatter
    {
        private readonly TFormatter _formatter;
        private readonly BindingReaderOptions _options;
        private readonly IObjectResolver _objectResolver;
        private readonly IRequestData _requestData;
        private readonly IServiceLocator _serviceLocator;

        public BindingReader(TFormatter formatter,
            BindingReaderOptions options,
            IObjectResolver objectResolver,
            IRequestData requestData,
            IServiceLocator serviceLocator)
        {
            _formatter = formatter;
            _options = options;
            _objectResolver = objectResolver;
            _requestData = requestData;
            _serviceLocator = serviceLocator;
        }

        public T Read(string mimeType)
        {
            var model = _formatter.Read<T>();
            var requestData = !_options.BindingSources.Any() ? _requestData :
                new RequestData(_options.BindingSources.Distinct()
                    .Select(x => _requestData.ValuesFor(x)));
            _objectResolver.BindProperties(typeof(T), model, 
                new BindingContext(requestData, _serviceLocator, new NulloBindingLogger()));
            return model;
        }

        public IEnumerable<string> Mimetypes
        {
            get { return _formatter.MatchingMimetypes; }
        }

        public void Describe(Description description)
        {
            var formatter = Description.For(_formatter);
            description.Title = DescriptionFormat.ToFormat(formatter.Title);
            description.Children["Formatter"] = formatter;
        }
    }
}
