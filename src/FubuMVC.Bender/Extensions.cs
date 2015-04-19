using System;
using System.Linq;
using System.Reflection;
using FubuMVC.Core.Http;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Resources.Conneg;
using FubuMVC.Core.View;

namespace FubuMVC.Bender
{
    public enum HttpMethod
    {
        Get, Head, Post, Put, Delete, Trace, Options, Connect, Patch, Unsupported
    }

    public static class Extensions
    {
        public static BehaviorChain ReplaceInputFormatters<T>(this BehaviorChain chain, Type reader) where T : IFormatter
        {
            var mimeTypes = typeof(T).GetCustomAttribute<MimeTypeAttribute>();
            if (chain.InputType() != null)
            {
                if (chain.Input.Readers != null && mimeTypes != null)
                {
                    var readerNode = chain.Input.Readers.FirstOrDefault(
                        x => x.Mimetypes.Any(y => mimeTypes.MimeTypes.Any(z => z == y)));
                    if (readerNode != null) readerNode.Remove();
                }
                chain.Input.Readers.AddToEnd(new Reader(reader
                    .MakeGenericType(chain.InputType(), typeof(T))));
            }
            return chain;
        }

        public static BehaviorChain ReplaceOutputFormatters<T>(this BehaviorChain chain) where T : IFormatter
        {
            var mimeTypes = typeof(T).GetCustomAttribute<MimeTypeAttribute>();
            if (chain.HasResourceType() && !chain.Output.Writers.Any(y => y is ViewNode))
            {
                if (chain.Output.Writers != null && mimeTypes != null)
                {
                    var writer = chain.Output.Writers.FirstOrDefault(
                        x => x.Mimetypes.Any(y => mimeTypes.MimeTypes.Any(z => z == y)));
                    if (writer != null) writer.Remove();
                }
                chain.Output.AddFormatter<T>();
            }
            return chain;
        }

        public static HttpMethod GetMethod(this ICurrentHttpRequest request)
        {
            return request.HttpMethod().ToHttpMethod();
        }

        public static HttpMethod ToHttpMethod(this string method)
        {
            if (method.IsNullOrEmpty()) return HttpMethod.Unsupported;
            switch (method.ToLower())
            {
                case "get": return HttpMethod.Get;
                case "head": return HttpMethod.Head;
                case "post": return HttpMethod.Post;
                case "put": return HttpMethod.Put;
                case "delete": return HttpMethod.Delete;
                case "trace": return HttpMethod.Trace;
                case "options": return HttpMethod.Options;
                case "connect": return HttpMethod.Connect;
                case "patch": return HttpMethod.Patch;
                default: return HttpMethod.Unsupported;
            }
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}
