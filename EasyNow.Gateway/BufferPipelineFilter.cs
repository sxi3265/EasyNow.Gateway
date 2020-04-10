using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public class BufferPipelineFilter : IPipelineFilter<HttpResponse>
    {
        private static ReadOnlyMemory<byte> _CRLF => new byte[] { (byte)'\r', (byte)'\n' };
        private static readonly ReadOnlyMemory<byte> _headerTerminator = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        
        public IPackageDecoder<HttpResponse> Decoder { get; set; }

        public IPipelineFilter<HttpResponse> NextFilter { get; internal set; }

        private HttpResponse _currentResponse;

        private long _bodyLength;

        private ReadOnlySequence<byte> total;
        private ReadOnlySequence<byte> body;
        private bool chunked;
        private int chunkedCurrentLength;

        public HttpResponse Filter(ref SequenceReader<byte> reader)
        {
            if (_currentResponse==null)
            {
                if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, _CRLF.Span, advancePastDelimiter: false))
                    return null;
                total = total.ConcactSequence(pack);
                total = total.ConcactSequence(new ReadOnlySequence<byte>(_CRLF) );

                reader.Advance(_CRLF.Length);
                var statusLine=pack.GetString(Encoding.UTF8);
                if (string.IsNullOrEmpty(statusLine))
                {
                    return null;
                }
                var metaInfos=statusLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                if (metaInfos.Length != 3)
                {
                    return null;
                }
                if (!reader.TryReadTo(out  pack, _headerTerminator.Span, advancePastDelimiter: false))
                    return null;
                total = total.ConcactSequence( pack);
                total = total.ConcactSequence(new ReadOnlySequence<byte>(_headerTerminator));
                reader.Advance(_headerTerminator.Length);
                var header = new HttpHeader(pack);
                if (!int.TryParse(metaInfos[1], out var statusCode))
                {
                    return null;
                }
                var response = new HttpResponse(metaInfos[0],statusCode,metaInfos[2],header);

                var transferEncoding = response.Header["transfer-encoding"];
                if (transferEncoding == "chunked")
                {
                    _currentResponse = response;
                    chunked = true;
                    return Filter(ref reader);
                }

                var contentLength = response.Header?["content-length"];

                if (string.IsNullOrEmpty(contentLength)) // no content
                    return response;

                var bodyLength = long.Parse(contentLength);

                if (bodyLength == 0)
                    return response;
                    
                _bodyLength = bodyLength;
                _currentResponse = response;

                return Filter(ref reader);
            }

            if (chunked)
            {
                if (chunkedCurrentLength == 0)
                {
                    if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, _CRLF.Span, advancePastDelimiter: false))
                        return null;
                    reader.Advance(_CRLF.Length);
                    chunkedCurrentLength = Convert.ToInt32($"0x{pack.GetString(Encoding.UTF8)}", 16);
                    body = body.ConcactSequence(pack);
                    body = body.ConcactSequence(new ReadOnlySequence<byte>(_CRLF));
                    if (chunkedCurrentLength == 0)
                    {
                        body = body.ConcactSequence(new ReadOnlySequence<byte>(_CRLF));
                        total = total.ConcactSequence(body,false);
                        _currentResponse.Data = total;
                        return _currentResponse;
                    }
                }
                if (reader.Remaining < chunkedCurrentLength)
                {
                    return null;
                }

                var chunkedPack=reader.Sequence.Slice(reader.Consumed, this.chunkedCurrentLength);
                body = body.ConcactSequence(chunkedPack);
                body = body.ConcactSequence(new ReadOnlySequence<byte>(_CRLF));
                reader.Advance(chunkedCurrentLength);
                reader.Advance(_CRLF.Length);
                chunkedCurrentLength = 0;
                return Filter(ref reader);
            }


            body = body.ConcactSequence(reader.Sequence.Slice(reader.Consumed, reader.Remaining));

            reader.Advance(reader.Remaining);

            if (body.Length < _bodyLength)
            {
                return null;
            }

            
            total = total.ConcactSequence(body,false);
            _currentResponse.Data = total;
            return _currentResponse;
        }

        public void Reset()
        {
            _bodyLength = 0;
            _currentResponse = null;
            chunked = false;
            chunkedCurrentLength = 0;
            total=new ReadOnlySequence<byte>();
            body=new ReadOnlySequence<byte>();
        }

        public object Context { get; set; }
    }
}