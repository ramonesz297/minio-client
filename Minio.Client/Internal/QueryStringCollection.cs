using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Minio.Client
{
    internal sealed class QueryStringCollection
    {
        private readonly SortedDictionary<string, string> _values = new SortedDictionary<string, string>();
        private readonly bool _encodeValues;

        internal QueryStringCollection(bool encodeValues = true)
        {
            _encodeValues = encodeValues;
        }

        internal QueryStringCollection(string query, bool encodeValues = true) : this(encodeValues)
        {
            var q = query.AsSpan().TrimStart('?');

            while (!q.IsEmpty)
            {
                ReadOnlySpan<char> segment;
                var delimeterIndex = q.IndexOf('&');
                if (delimeterIndex >= 0)
                {
                    segment = q.Slice(0, delimeterIndex);
                    q = q.Slice(delimeterIndex + 1);
                }
                else
                {
                    segment = q;
                    q = default;
                }

                var equalIndex = segment.IndexOf('=');
                if (equalIndex >= 0)
                {
                    Add(segment.Slice(0, equalIndex).ToString(), segment.Slice(equalIndex + 1).ToString());
                }
                else if (!segment.IsEmpty)
                {
                    Add(segment.ToString(), "");
                }
            }
        }

        public void Encode()
        {
            foreach (var item in _values)
            {
                _values[item.Key] = PrepareValue(item.Value);
            }
        }

        private string PrepareValue(string value)
        {
            return _encodeValues ? WebUtility.UrlEncode(value) : value;
        }

        public void Add(string key, string value)
        {
            if (_values.ContainsKey(key))
            {
                _values[key] = PrepareValue(value);
            }
            else
            {
                _values.Add(key, PrepareValue(value));
            }
        }

        public void Clear()
        {
            _values.Clear();
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(char? prefix)
        {
            if (_values.Count == 0)
            {
                return "";
            }

            var sb = new StringBuilder();

            if (prefix.HasValue)
            {
                sb.Append(prefix);
            }

            int count = 0;

            foreach (var item in _values)
            {
                if (count > 0)
                {
                    sb.Append('&');
                }
                sb.Append(item.Key).Append('=').Append(item.Value);
                count++;
            }

            return sb.ToString();
        }

    }
}