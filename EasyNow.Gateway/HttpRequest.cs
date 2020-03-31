using System.Text;

namespace EasyNow.Gateway
{
    public class HttpRequest
    {
        private static readonly char SPACE = ' ';
        private static readonly string CRLF = "\r\n";
        private static readonly char COLON = ':';

        public string Method { get; }

        public string Path { get; }

        public string HttpVersion { get; }

        public HttpHeader Header { get; }

        public string Body { get; set; }

        public HttpRequest(string method,string path,string httpVersion,HttpHeader header)
        {
            Method = method;
            Path = path;
            HttpVersion = httpVersion;
            Header = header;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Method);
            sb.Append(SPACE);
            sb.Append(Path);
            sb.Append(SPACE);
            sb.Append(HttpVersion);
            sb.Append(CRLF);
            foreach (var key in Header.AllKeys)
            {
                sb.Append(key);
                sb.Append(COLON);
                sb.Append(SPACE);
                sb.Append(Header[key]);
                sb.Append(CRLF);
            }

            sb.Append(CRLF);
            sb.Append(Body);
            return sb.ToString();
        }
    }
}