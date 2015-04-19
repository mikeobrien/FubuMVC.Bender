using System;

namespace FubuMVC.Bender
{
    public interface IFormatter : Core.Runtime.Formatters.IFormatter
    {
        object Read(Type type);
        void Write(object target, string mimeType);
    }
}
