using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Serialization
{
    /// <summary>
    /// A helper class to serialize intermediate serialization objects to a binary stream.
    /// </summary>
    public class SBinaryFormatter
    {
        /// <summary>
        /// Helper class for field type headers.
        /// </summary>
        public static class FieldType
        {
            public const byte NULL = 0x00;
            public const byte START_OBJ = 0x01;
            public const byte END_OBJ = 0x02;
            public const byte START_ARR = 0x03;
            public const byte END_ARR = 0x04;
            public const byte SHORT_STRING = 0x10;
            public const byte STRING = 0x11;
            public const byte LONG_STRING = 0x12;
            public const byte BLOB = 0x20;
            public const byte INT = 0x30;
            public const byte UINT = 0x31;
            public const byte FLOAT = 0x32;
            public const byte DECIMAL = 0x33;
            public const byte DATETIME = 0x40;
            public const byte DATETIME_OFFSET = 0x41;
            public const byte BOOLEAN = 0x50;
        }
        private byte[] buffer;
        private void EnsureBufferLength(int capacity)
        {
            if (buffer == null || buffer.Length < capacity)
                buffer = new byte[capacity];
        }
        private async Task Read(Stream stream, int len, byte[] buf = null)
        {
            if (buf == null)
                EnsureBufferLength(len);
            buf = buf ?? buffer;
            var pos = 0;
            var read = -1;
            while (pos < len && read != 0)
            {
                read = await stream.ReadAsync(buf, pos, len - pos);
                pos += read;
            }
            if (pos < len)
                throw new EndOfStreamException();
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public SBinaryFormatter()
        {
        }
        #region Reading
        /// <summary>
        /// Reads an SItem from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>An asynchronous SItem.</returns>
        public async Task<SItem> ReadItem(Stream stream)
        {
            await Read(stream, 1);
            return await ReadItem(buffer[0], stream);
        }

        private async Task<SObject> ReadObject(Stream stream)
        {
            byte header;
            var props = new List<SProperty>();
            do
            {
                await Read(stream, 1);
                header = buffer[0];
                if (header != FieldType.END_OBJ)
                {
                    var propname = await ReadString(header, stream);
                    var val = await ReadItem(stream);
                    await Read(stream, 1);
                    var fmt = buffer[0];
                    props.Add(new SProperty(propname, val, (SFormat)fmt));
                }
            } while (header != FieldType.END_OBJ);
            return new SObject(props);
        }
        private async Task<SArray> ReadArray(Stream stream)
        {
            byte header;
            var vals = new List<SItem>();
            do
            {
                await Read(stream, 1);
                header = buffer[0];
                if (header != FieldType.END_ARR)
                {
                    var val = await ReadItem(header, stream);
                    vals.Add(val);
                }

            } while (header != FieldType.END_ARR);
            return new SArray(vals);
        }
        private async Task<SItem> ReadItem(byte header, Stream stream)
        {
            switch (header)
            {
                case FieldType.NULL:
                    return SValue.Null;
                case FieldType.START_OBJ:
                    return await ReadObject(stream);
                case FieldType.START_ARR:
                    return await ReadArray(stream);
                default:
                    return await ReadValue(header, stream);
            }
        }
        private async Task<string> ReadString(byte header, Stream stream)
        {
            switch (header)
            {
                case FieldType.SHORT_STRING:
                    await Read(stream, 1);
                    var slen = buffer[0];
                    await Read(stream, slen);
                    return Encoding.UTF8.GetString(buffer, 0, slen);
                case FieldType.STRING:
                    await Read(stream, 2);
                    var len = BitConverter.ToUInt16(buffer, 0);
                    await Read(stream, len);
                    return Encoding.UTF8.GetString(buffer, 0, len);
                case FieldType.LONG_STRING:
                    await Read(stream, 4);
                    var wlen = BitConverter.ToInt32(buffer, 0);
                    await Read(stream, wlen);
                    return Encoding.UTF8.GetString(buffer, 0, wlen);
                default:
                    throw new InvalidDataException();

            }
        }
        private async Task<SValue> ReadValue(byte header, Stream stream)
        {
            switch (header)
            {
                case FieldType.SHORT_STRING:
                case FieldType.STRING:
                case FieldType.LONG_STRING:
                    return new SValue(await ReadString(header, stream));
                case FieldType.BLOB:
                    await Read(stream, 4);
                    var wlen = BitConverter.ToInt32(buffer, 0);
                    var res = new byte[wlen];
                    await Read(stream, wlen, res);
                    return new SValue(res);
                case FieldType.INT:
                    await Read(stream, 1);
                    var slen = buffer[0];
                    await Read(stream, slen);
                    switch (slen)
                    {
                        case 1:
                            unchecked
                            {
                                return new SValue((sbyte)buffer[0]);
                            }
                        case 2:
                            return new SValue(BitConverter.ToInt16(buffer, 0));
                        case 4:
                            return new SValue(BitConverter.ToInt32(buffer, 0));
                        case 8:
                            return new SValue(BitConverter.ToInt64(buffer, 0));
                        default:
                            throw new InvalidDataException();
                    }
                case FieldType.UINT:
                    await Read(stream, 1);
                    slen = buffer[0];
                    await Read(stream, slen);
                    switch (slen)
                    {
                        case 1:
                            return new SValue(buffer[0]);
                        case 2:
                            return new SValue(BitConverter.ToUInt16(buffer, 0));
                        case 4:
                            return new SValue(BitConverter.ToUInt32(buffer, 0));
                        case 8:
                            return new SValue(BitConverter.ToUInt64(buffer, 0));
                        default:
                            throw new InvalidDataException();
                    }
                case FieldType.FLOAT:
                    await Read(stream, 1);
                    slen = buffer[0];
                    await Read(stream, slen);
                    switch (slen)
                    {
                        case 4:
                            return new SValue(BitConverter.ToSingle(buffer, 0));
                        case 8:
                            return new SValue(BitConverter.ToDouble(buffer, 0));
                        default:
                            throw new InvalidDataException();
                    }
                case FieldType.DECIMAL:
                    await Read(stream, 1);
                    if (buffer[0] != 16)
                        throw new InvalidDataException();
                    await Read(stream, 16);
                    return new SValue(new decimal(new int[]
                    {
                        BitConverter.ToInt32(buffer,0),
                        BitConverter.ToInt32(buffer,4),
                        BitConverter.ToInt32(buffer,8),
                        BitConverter.ToInt32(buffer,12)
                    }));
                case FieldType.DATETIME:
                    await Read(stream, 1);
                    if (buffer[0] != 8)
                        throw new InvalidDataException();
                    await Read(stream, 8);
                    return new SValue(new DateTime(BitConverter.ToInt64(buffer, 0), DateTimeKind.Utc));
                case FieldType.DATETIME_OFFSET:
                    await Read(stream, 1);
                    if (buffer[0] != 16)
                        throw new InvalidDataException();
                    await Read(stream, 16);
                    return new SValue(new DateTimeOffset(new DateTime(BitConverter.ToInt64(buffer, 0)), new TimeSpan(BitConverter.ToInt64(buffer, 8))));
                case FieldType.BOOLEAN:
                    await Read(stream, 1);
                    return new SValue(buffer[0] != 0);
                default:
                    throw new InvalidDataException();
            }
        }
        #endregion

        #region Writing
        /// <summary>
        /// Writes an SItem to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="item">The SItem instance to write to the stream.</param>
        /// <returns>A Task representing the asynchronous request.</returns>
        public Task WriteItem(Stream stream, SItem item)
        {
            switch (item)
            {
                case SObject o:
                    return WriteObject(stream, o);
                case SArray a:
                    return WriteArray(stream, a);
                case SValue v:
                    return WriteValue(stream, v);
                default:
                    return Task.CompletedTask;
            }
        }

        private async Task WriteString(Stream stream, string s)
        {
            if (s.Length <= byte.MaxValue)
            {
                EnsureBufferLength(2);
                buffer[0] = FieldType.SHORT_STRING;
                buffer[1] = (byte)s.Length;
                await stream.WriteAsync(buffer, 0, 2);
            }
            else if (s.Length <= ushort.MaxValue)
            {
                EnsureBufferLength(3);
                buffer[0] = FieldType.STRING;
                BitConverter.GetBytes((ushort)s.Length).CopyTo(buffer, 1);
                await stream.WriteAsync(buffer, 0, 3);
            }
            else
            {
                EnsureBufferLength(5);
                buffer[0] = FieldType.LONG_STRING;
                BitConverter.GetBytes(s.Length).CopyTo(buffer, 1);
                await stream.WriteAsync(buffer, 0, 5);
            }
            var str = Encoding.UTF8.GetBytes(s);
            await stream.WriteAsync(str, 0, str.Length);
        }
        private async Task WriteValue(Stream stream, SValue v)
        {
            EnsureBufferLength(1);
            if (v.Value == null)
            {
                buffer[0] = FieldType.NULL;
                await stream.WriteAsync(buffer, 0, 1);

            }
            else
            {
                switch (v.Value)
                {
                    case string s:
                        await WriteString(stream, s);
                        break;
                    case byte[] bin:
                        EnsureBufferLength(5);
                        buffer[0] = FieldType.BLOB;
                        BitConverter.GetBytes(bin.Length).CopyTo(buffer, 1);
                        await stream.WriteAsync(buffer, 0, 5);
                        await stream.WriteAsync(bin, 0, bin.Length);
                        break;
                    case int i:
                        EnsureBufferLength(6);
                        buffer[0] = FieldType.INT;
                        buffer[1] = 4;
                        BitConverter.GetBytes(i).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 6);
                        break;
                    case long l:
                        EnsureBufferLength(10);
                        buffer[0] = FieldType.INT;
                        buffer[1] = 8;
                        BitConverter.GetBytes(l).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 10);
                        break;
                    case short s:
                        EnsureBufferLength(4);
                        buffer[0] = FieldType.INT;
                        buffer[1] = 2;
                        BitConverter.GetBytes(s).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 4);
                        break;
                    case sbyte sb:
                        EnsureBufferLength(3);
                        buffer[0] = FieldType.INT;
                        buffer[1] = 1;
                        unchecked
                        {
                            buffer[2] = (byte)sb;
                        }
                        await stream.WriteAsync(buffer, 0, 3);
                        break;
                    case byte b:
                        EnsureBufferLength(3);
                        buffer[0] = FieldType.UINT;
                        buffer[1] = 1;
                        buffer[2] = b;
                        await stream.WriteAsync(buffer, 0, 3);
                        break;
                    case uint i:
                        EnsureBufferLength(6);
                        buffer[0] = FieldType.UINT;
                        buffer[1] = 4;
                        BitConverter.GetBytes(i).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 6);
                        break;
                    case ulong l:
                        EnsureBufferLength(10);
                        buffer[0] = FieldType.UINT;
                        buffer[1] = 8;
                        BitConverter.GetBytes(l).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 10);
                        break;
                    case ushort s:
                        EnsureBufferLength(4);
                        buffer[0] = FieldType.UINT;
                        buffer[1] = 2;
                        BitConverter.GetBytes(s).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 4);
                        break;
                    case decimal d:
                        EnsureBufferLength(18);
                        buffer[0] = FieldType.DECIMAL;
                        buffer[1] = 16;
                        var ints = decimal.GetBits(d);
                        for (var i = 0; i < 4; i++)
                            BitConverter.GetBytes(ints[i]).CopyTo(buffer, 2 + (i << 2));
                        await stream.WriteAsync(buffer, 0, 18);
                        break;
                    case double d:
                        EnsureBufferLength(10);
                        buffer[0] = FieldType.FLOAT;
                        buffer[1] = 8;
                        BitConverter.GetBytes(d).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 10);
                        break;
                    case float f:
                        EnsureBufferLength(6);
                        buffer[0] = FieldType.FLOAT;
                        buffer[1] = 4;
                        BitConverter.GetBytes(f).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 6);
                        break;
                    case DateTime dt:
                        EnsureBufferLength(10);
                        buffer[0] = FieldType.DATETIME;
                        buffer[1] = 8;
                        BitConverter.GetBytes(dt.ToUniversalTime().Ticks).CopyTo(buffer, 2);
                        await stream.WriteAsync(buffer, 0, 10);
                        break;
                    case DateTimeOffset dto:
                        EnsureBufferLength(18);
                        buffer[0] = FieldType.DATETIME_OFFSET;
                        buffer[1] = 16;
                        BitConverter.GetBytes(dto.Ticks).CopyTo(buffer, 2);
                        BitConverter.GetBytes(dto.Offset.Ticks).CopyTo(buffer, 10);
                        await stream.WriteAsync(buffer, 0, 18);
                        break;
                    case bool bl:
                        EnsureBufferLength(2);
                        buffer[0] = FieldType.BOOLEAN;
                        buffer[1] = (byte)(bl ? 1 : 0);
                        await stream.WriteAsync(buffer, 0, 2);
                        break;
                    default:
                        throw new InvalidDataException($"Cannot serialize type {v.Value.GetType().FullName}");
                }

            }
        }
        private async Task WriteArray(Stream stream, SArray a)
        {
            EnsureBufferLength(1);
            buffer[0] = FieldType.START_ARR;
            await stream.WriteAsync(buffer, 0, 1);
            foreach (var val in a.Content)
                await WriteItem(stream, val);
            buffer[0] = FieldType.END_ARR;
            await stream.WriteAsync(buffer, 0, 1);
        }
        private async Task WriteObject(Stream stream, SObject o)
        {
            EnsureBufferLength(1);
            buffer[0] = FieldType.START_OBJ;
            await stream.WriteAsync(buffer, 0, 1);
            foreach (var prop in o.Properties)
            {
                await WriteString(stream, prop.Name);
                await WriteItem(stream, prop.Token);
                buffer[0] = (byte)prop.Format;
                await stream.WriteAsync(buffer, 0, 1);
            }
            buffer[0] = FieldType.END_OBJ;
            await stream.WriteAsync(buffer, 0, 1);
        }
        #endregion
    }
}
