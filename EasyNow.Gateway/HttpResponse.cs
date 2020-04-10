using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public class HttpResponse
    {
        private static readonly char SPACE = ' ';
        private static readonly string CRLF = "\r\n";
        private static readonly char COLON = ':';

        public int StatusCode { get; }

        public string ReasonPhrase{ get; }

        public string HttpVersion { get; }

        public HttpHeader Header { get; }

        public ReadOnlySequence<byte> Data { get; set; }

        public HttpResponse(string httpVersion,int statusCode,string reasonPhrase,HttpHeader httpHeader)
        {
            HttpVersion = httpVersion;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Header = httpHeader;
        }

        public void BuildData()
        {
            var sb = new StringBuilder();
            sb.Append(HttpVersion);
            sb.Append(SPACE);
            sb.Append(StatusCode);
            sb.Append(SPACE);
            sb.Append(ReasonPhrase);
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
            var writer=new ArrayBufferWriter<byte>();
            writer.Write(sb.ToString(),Encoding.UTF8);
            Data=new ReadOnlySequence<byte>(writer.WrittenMemory);
        }
    }
}