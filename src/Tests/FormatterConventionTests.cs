using System.Linq;
using Bender.Collections;
using FubuMVC.Bender;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Resources.Conneg;
using FubuMVC.Core.View;
using NSubstitute;
using NUnit.Framework;
using Should;
using FubuXmlFormatter = FubuMVC.Core.Runtime.Formatters.XmlFormatter;
using FubuJsonFormatter = FubuMVC.Core.Runtime.Formatters.JsonFormatter;

namespace Tests
{
    [TestFixture]
    public class FormatterConventionTests
    {
        public class Request { }
        public class Response { }

        public class InputOutputHandler
        {
            public Response Execute(Request request) { return null; }
        }

        public class InputHandler
        {
            public void Execute(Request request) { }
        }

        public class OutputHandler
        {
            public Response Execute() { return null; }
        }

        private BehaviorGraph CreateGraph<T>()
        {
            var graph = new BehaviorGraph();
            graph.AddActionFor("/", typeof(T));
            graph.AddActionFor("/", typeof(T));
            return graph;
        }

        private void should_only_have_bender_readers(BehaviorGraph graph)
        {
            graph.Behaviors.All(x => 
                    x.Input.Readers.OfType<Reader>().Any(y => y.ReaderType == 
                        typeof(BindingReader<Request, JsonFormatter>)))
                .ShouldBeTrue();

            graph.Behaviors.All(x =>
                    x.Input.Readers.OfType<Reader>().Any(y => y.ReaderType ==
                        typeof(BindingReader<Request, XmlFormatter>)))
                .ShouldBeTrue();
            
            graph.Behaviors.All(x =>
                    x.Input.Readers.OfType<Reader>().Any(y => y.ReaderType ==
                        typeof(FormatterReader<Request, FubuJsonFormatter>)))
                .ShouldBeFalse();

            graph.Behaviors.All(x =>
                    x.Input.Readers.OfType<Reader>().Any(y => y.ReaderType ==
                        typeof(FormatterReader<Request, FubuXmlFormatter>)))
                .ShouldBeFalse();
        }

        private void should_only_have_bender_writers(BehaviorGraph graph)
        {
            graph.Behaviors.All(x =>
                    x.Output.Writers.OfType<WriteWithFormatter>().Any(y => 
                        y.FormatterType == typeof(JsonFormatter)))
                .ShouldBeTrue();

            graph.Behaviors.All(x =>
                    x.Output.Writers.OfType<WriteWithFormatter>().Any(y => 
                        y.FormatterType == typeof(XmlFormatter)))
                .ShouldBeTrue();

            graph.Behaviors.All(x =>
                    x.Output.Writers.OfType<WriteWithFormatter>().Any(y => 
                        y.FormatterType == typeof(FubuJsonFormatter)))
                .ShouldBeFalse();

            graph.Behaviors.All(x =>
                    x.Output.Writers.OfType<WriteWithFormatter>().Any(y => 
                        y.FormatterType == typeof(FubuXmlFormatter)))
                .ShouldBeFalse();
        }

        [Test]
        public void should_add_input_and_output_formatters()
        {
            var graph = CreateGraph<InputOutputHandler>();

            new FormatterConvention().Configure(graph);

            should_only_have_bender_readers(graph);
            should_only_have_bender_writers(graph);
        }

        [Test]
        public void should_add_input_formatters_only()
        {
            var graph = CreateGraph<InputHandler>();

            new FormatterConvention().Configure(graph);

            should_only_have_bender_readers(graph);
        }

        [Test]
        public void should_add_output_formatters_only()
        {
            var graph = CreateGraph<OutputHandler>();

            new FormatterConvention().Configure(graph);

            should_only_have_bender_writers(graph);
        }

        [Test]
        public void should_replace_input_and_output_formatters()
        {
            var graph = CreateGraph<InputOutputHandler>();

            AddStockReaders(graph);
            AddStockWriters(graph);

            new FormatterConvention().Configure(graph);

            should_only_have_bender_readers(graph);
            should_only_have_bender_writers(graph);
        }

        [Test]
        public void should_replace_input_formatters_only()
        {
            var graph = CreateGraph<InputHandler>();

            AddStockReaders(graph);

            new FormatterConvention().Configure(graph);

            should_only_have_bender_readers(graph);
        }

        [Test]
        public void should_replace_output_formatters_only()
        {
            var graph = CreateGraph<OutputHandler>();

            AddStockWriters(graph);

            new FormatterConvention().Configure(graph);

            should_only_have_bender_writers(graph);
        }

        public static void AddStockReaders(BehaviorGraph graph)
        {
            graph.Behaviors.ForEach(x =>
            {
                x.Input.AddFormatter<FubuXmlFormatter>();
                x.Input.AddFormatter<FubuJsonFormatter>();
            });
        }

        public static void AddStockWriters(BehaviorGraph graph)
        {
            graph.Behaviors.ForEach(x =>
            {
                x.Output.AddFormatter<FubuXmlFormatter>();
                x.Output.AddFormatter<FubuJsonFormatter>();
            });
        }

        [Test]
        public void should_not_replace_or_add_when_there_is_a_view_node()
        {
            var graph = CreateGraph<OutputHandler>();

            graph.Behaviors.ForEach(x => x.Output.AddView(Substitute.For<IViewToken>()));

            new FormatterConvention().Configure(graph);

            graph.Behaviors.All(x =>
                    x.Output.Writers.All(y => y is ViewNode))
                .ShouldBeTrue();
        }
    }
}
