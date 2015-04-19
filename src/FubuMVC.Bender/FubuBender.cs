using System;
using Bender.Configuration;
using FubuMVC.Core;
using FubuMVC.Core.Http;

namespace FubuMVC.Bender
{
    public class FubuBender : IFubuRegistryExtension
    {
        private readonly Options _options;
        private readonly BindingReaderOptions _bindingReaderOptions;

        public FubuBender()
        {
            _options = new Options();
            _bindingReaderOptions = new BindingReaderOptions();
        }

        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            registry.Services(x => x.AddService(_options));
            registry.Services(x => x.AddService(_bindingReaderOptions));
            registry.Policies.Add<FormatterConvention>();
        }

        public class BindingDsl
        {
            private readonly BindingReaderOptions _options;

            public BindingDsl(BindingReaderOptions options)
            {
                _options = options;
            }

            public BindingDsl BindRequest()
            {
                _options.BindingSources.Add(RequestDataSource.Request);
                return this;
            }

            public BindingDsl BindRequestProperties()
            {
                _options.BindingSources.Add(RequestDataSource.RequestProperty);
                return this;
            }

            public BindingDsl BindFiles()
            {
                _options.BindingSources.Add(RequestDataSource.File);
                return this;
            }

            public BindingDsl BindHeaders()
            {
                _options.BindingSources.Add(RequestDataSource.Header);
                return this;
            }

            public BindingDsl BindCookies()
            {
                _options.BindingSources.Add(RequestDataSource.Cookie);
                return this;
            }

            public BindingDsl BindOther()
            {
                _options.BindingSources.Add(RequestDataSource.Other);
                return this;
            }
        }

        public FubuBender Bindings(Action<BindingDsl> config)
        {
            config(new BindingDsl(_bindingReaderOptions));
            return this;
        }

        public FubuBender Configure(Action<OptionsDsl> config)
        {
            config(new OptionsDsl(_options));
            return this;
        }
    }
}