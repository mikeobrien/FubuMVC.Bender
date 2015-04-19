using System.IO;
using System.Text;

namespace Tests
{
    public static class Extensions
    {
        public static bool IsNotNull(this object value)
        {
            return value != null;
        }

        public static Stream ToAsciiStream(this string value)
        {
            return new MemoryStream(Encoding.ASCII.GetBytes(value));
        }

        public static string ReadAllText(this Stream stream)
        {
            lock (stream)
            {
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
                return new StreamReader(stream).ReadToEnd();
            }
        }
    }
}
