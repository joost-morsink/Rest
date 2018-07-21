using Biz.Morsink.Identity.PathProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.HttpConverter.Utils
{
    public class ResettableJsonReader : JsonReader
    {
        private readonly JsonReader reader;
        private readonly List<Token> tokens;
        private int position;
        private bool resettable;
        private readonly Token preToken;
        public ResettableJsonReader(JsonReader inner)
        {
            reader = inner;
            tokens = new List<Token>();
            resettable = true;
            position = -1;
            preToken = new Token(reader);
        }
        private struct Token
        {
            public Token(JsonReader r)
                : this(r.TokenType, r.ValueType, r.Value, r.Path, r.Depth)
            { }
            public Token(JsonToken tokenType, Type valueType, object value, string path, int depth)
            {
                TokenType = tokenType;
                ValueType = valueType;
                Value = value;
                Path = path;
                Depth = depth;
            }
            public JsonToken TokenType { get; }
            public object Value { get; }
            public Type ValueType { get; }
            public string Path { get; }
            public int Depth { get; }
        }
        public override bool Read()
        {
            if (++position >= tokens.Count)
            {
                if (reader.Read())
                {
                    tokens.Add(new Token(reader));
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }
        public void Reset(bool allowNextReset = false)
        {
            if (!resettable)
                throw new InvalidOperationException("Reader is not resettable.");
            position = -1;
            resettable = allowNextReset;
        }
        public async override Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (++position >= tokens.Count)
            {
                if (await reader.ReadAsync())
                {
                    tokens.Add(new Token(reader));
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }
        public override string Path
            => position < 0
                ? preToken.Path
                : position < tokens.Count
                    ? tokens[position].Path
                    : reader.Path;
        public override JsonToken TokenType
            => position < 0
                ? preToken.TokenType
                : position < tokens.Count
                    ? tokens[position].TokenType
                    : reader.TokenType;
        public override Type ValueType
            => position < 0
                ? preToken.ValueType
                : position < tokens.Count
                    ? tokens[position].ValueType
                    : reader.ValueType;
        public override object Value
            => position < 0
                ? preToken.Value
                : position < tokens.Count
                    ? tokens[position].Value
                    : reader.Value;
        public override int Depth
            => position < 0
                ? preToken.Depth
                : position < tokens.Count
                    ? tokens[position].Depth
                    : reader.Depth;

        public bool HasProperty(int level, string name)
        {
            int depth = 0;
            bool success = false;
            do
            {
                switch (TokenType)
                {
                    case JsonToken.StartObject:
                        depth++;
                        break;
                    case JsonToken.EndObject:
                        depth--;
                        break;
                    case JsonToken.PropertyName:
                        if (depth == level && CaseInsensitiveEqualityComparer.Instance.Equals(Value?.ToString(), name))
                            success = true;
                        break;
                }
                if (depth > 0)
                    Read();
            } while (depth > 0 && !success);
            Reset(true);
            return success;

        }
    }
}
