using Bender.Collections;
using FubuMVC.Core;
using FubuMVC.Core.Registration;

namespace FubuMVC.Bender
{
    [ConfigurationType(ConfigurationType.Attachment)]
    public class FormatterConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.Behaviors.ForEach(x => x
                .ReplaceInputFormatters<XmlFormatter>(typeof(BindingReader<,>))
                .ReplaceOutputFormatters<XmlFormatter>()
                .ReplaceInputFormatters<JsonFormatter>(typeof(BindingReader<,>))
                .ReplaceOutputFormatters<JsonFormatter>());
        }
    }
}
