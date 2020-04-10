using System;
using System.Buffers;

namespace EasyNow.Gateway
{
    public class HttpResponse
    {
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
    }
}