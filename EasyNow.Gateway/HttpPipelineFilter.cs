using System;
using System.Buffers;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public class HttpPipelineFilter : IPipelineFilter<HttpRequest>
    {
        private static ReadOnlySpan<byte> _CRLF => new byte[] { (byte)'\r', (byte)'\n' };
        
        private static readonly char _TAB = '\t';

        private static readonly char _COLON = ':';

        private static readonly ReadOnlyMemory<byte> _headerTerminator = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        
        public IPackageDecoder<HttpRequest> Decoder { get; set; }

        public IPipelineFilter<HttpRequest> NextFilter { get; internal set; }

        private HttpRequest _currentRequest;

        private long _bodyLength;

        public HttpRequest Filter(ref SequenceReader<byte> reader)
        {
            if (_bodyLength == 0)
            {
                var terminatorSpan = _headerTerminator.Span;

                if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, _CRLF, advancePastDelimiter: false))
                    return null;

                reader.Advance(_CRLF.Length);
                var requestLine=pack.GetString(Encoding.UTF8);
                if (string.IsNullOrEmpty(requestLine))
                {
                    return null;
                }
                var metaInfos=requestLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                if (metaInfos.Length != 3)
                {
                    return null;
                }
                if (!reader.TryReadTo(out  pack, terminatorSpan, advancePastDelimiter: false))
                    return null;
                reader.Advance(terminatorSpan.Length);
                var header = new HttpHeader(pack);

                var request = new HttpRequest(metaInfos[0],metaInfos[1],metaInfos[2],header);

                var contentLength = request.Header["content-length"];

                if (string.IsNullOrEmpty(contentLength)) // no content
                    return request;

                var bodyLength = long.Parse(contentLength);

                if (bodyLength == 0)
                    return request;
                    
                _bodyLength = bodyLength;
                _currentRequest = request;

                return Filter(ref reader);
            }

            if (reader.Remaining < _bodyLength)
                return null;

            var seq= reader.Sequence;

            _currentRequest.Body = seq.Slice(reader.Consumed, _bodyLength).GetString(Encoding.UTF8);
            reader.Advance(_bodyLength);

            return _currentRequest;
        }

        public void Reset()
        {
            _bodyLength = 0;
            _currentRequest = null;
        }

        public object Context { get; set; }
    }
}