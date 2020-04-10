using System;
using System.Buffers;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public class HttpHeader
    {
        private static readonly char _TAB = '\t';
        private static readonly char _COLON = ':';

        public ReadOnlySequence<byte> Data { get; }

        private readonly NameValueCollection _items;

        public string[] AllKeys => _items.AllKeys;

        public HttpHeader(NameValueCollection nameValueCollection)
        {
            _items = nameValueCollection;
        }

        public HttpHeader(ReadOnlySequence<byte> data)
        {
            Data = data;

            _items = new NameValueCollection();
            var stringReader = new StringReader(Data.GetString(Encoding.UTF8));

            var line = string.Empty;
            var prevKey = string.Empty;
            while (!string.IsNullOrEmpty(line = stringReader.ReadLine()))
            {
                if (line.StartsWith(_TAB) && !string.IsNullOrEmpty(prevKey))
                {
                    var currentValue = _items.Get(prevKey);
                    _items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(_COLON);

                if (pos <= 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                if (string.IsNullOrEmpty(key))
                    continue;

                var valueOffset = pos + 1;

                if (line.Length <= valueOffset) //No value in this line
                    continue;

                var value = line.Substring(valueOffset);

                if (!string.IsNullOrEmpty(value) && value.StartsWith(' ') && value.Length > 1)
                    value = value.Substring(1);

                var existingValue = _items.Get(key);

                if (string.IsNullOrEmpty(existingValue))
                {
                    _items.Add(key, value);
                }
                else
                {
                    _items[key] = existingValue + ", " + value;
                }

                prevKey = key;
            }
        }

        public string this[string name]
        {
            get => _items[name];
            set => _items[name] = value;
        }
    }
}