using System;
using System.Collections.Generic;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for DER encoded data.
    /// </summary>
    public class DerData
    {
        private const byte Constructed = 0x20;

        private const byte Boolean = 0x01;
        private const byte Integer = 0x02;
        private const byte BITSTRING = 0x03;
        private const byte Octetstring = 0x04;
        private const byte Null = 0x05;
        private const byte Objectidentifier = 0x06;
        //private const byte EXTERNAL = 0x08;
        //private const byte ENUMERATED = 0x0a;
        private const byte Sequence = 0x10;
        //private const byte SEQUENCEOF = 0x10; // for completeness
        //private const byte SET = 0x11;
        //private const byte SETOF = 0x11; // for completeness

        //private const byte NUMERICSTRING = 0x12;
        //private const byte PRINTABLESTRING = 0x13;
        //private const byte T61STRING = 0x14;
        //private const byte VIDEOTEXSTRING = 0x15;
        //private const byte IA5STRING = 0x16;
        //private const byte UTCTIME = 0x17;
        //private const byte GENERALIZEDTIME = 0x18;
        //private const byte GRAPHICSTRING = 0x19;
        //private const byte VISIBLESTRING = 0x1a;
        //private const byte GENERALSTRING = 0x1b;
        //private const byte UNIVERSALSTRING = 0x1c;
        //private const byte BMPSTRING = 0x1e;
        //private const byte UTF8STRING = 0x0c;
        //private const byte APPLICATION = 0x40;
        //private const byte TAGGED = 0x80;

        private readonly List<byte> _data;

        private int _readerIndex;
        private readonly int _lastIndex;

        /// <summary>
        /// Gets a value indicating whether end of data is reached.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if end of data is reached; otherwise, <c>false</c>.
        /// </value>
        public bool IsEndOfData
        {
            get
            {
                return _readerIndex >= _lastIndex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerData"/> class.
        /// </summary>
        public DerData()
        {
            _data = new List<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerData"/> class.
        /// </summary>
        /// <param name="data">DER encoded data.</param>
        /// <param name="construct">its a construct</param>
        public DerData(byte[] data, bool construct = false)
        {
            _data = new List<byte>(data);
            if (construct)
            {
                _lastIndex = _readerIndex + data.Length;
            }
            else
            {
                ReadByte(); // skip dataType
                var length = ReadLength();
                _lastIndex = _readerIndex + length;
            }
        }

        /// <summary>
        /// Encodes written data as DER byte array.
        /// </summary>
        /// <returns>DER Encoded array.</returns>
        public byte[] Encode()
        {
            var length = _data.Count;
            var lengthBytes = GetLength(length);

            _data.InsertRange(0, lengthBytes);
            _data.Insert(0, Constructed | Sequence);

            return _data.ToArray();
        }

        /// <summary>
        /// Reads next mpint data type from internal buffer.
        /// </summary>
        /// <returns>mpint read.</returns>
        public BigInteger ReadBigInteger()
        {
            var type = ReadByte();
            if (type != Integer)
                throw new InvalidOperationException(string.Format("Invalid data type, INTEGER(02) is expected, but was {0}", type.ToString("X2")));

            var length = ReadLength();

            var data = ReadBytes(length);

            return new BigInteger(data.Reverse());
        }

        /// <summary>
        /// Reads next int data type from internal buffer.
        /// </summary>
        /// <returns>int read.</returns>
        public int ReadInteger()
        {
            var type = ReadByte();
            if (type != Integer)
                throw new InvalidOperationException(string.Format("Invalid data type, INTEGER(02) is expected, but was {0}", type.ToString("X2")));

            var length = ReadLength();

            var data = ReadBytes(length);

            if (length > 4)
                throw new InvalidOperationException("Integer type cannot occupy more then 4 bytes");

            var result = 0;
            var shift = (length - 1) * 8;
            for (var i = 0; i < length; i++)
            {
                result |= data[i] << shift;
                shift -= 8;
            }

            //return (int)(data[0] << 56 | data[1] << 48 | data[2] << 40 | data[3] << 32 | data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7]);

            return result;
        }

        /// <summary>
        /// Reads next octetstring data type from internal buffer.
        /// </summary>
        /// <returns>data read.</returns>
        public byte[] ReadOctetString()
        {
            var type = ReadByte();
            if (type != Octetstring)
                throw new InvalidOperationException(string.Format("Invalid data type, OCTETSTRING(04) is expected, but was {0}", type.ToString("X2")));

            var length = ReadLength();
            var data = ReadBytes(length);
            return data;
        }

        /// <summary>
        /// Reads next bitstring data type from internal buffer.
        /// </summary>
        /// <returns>data read.</returns>
        public byte[] ReadBitString()
        {
            var type = ReadByte();
            if (type != BITSTRING)
                throw new InvalidOperationException(string.Format("Invalid data type, BITSTRING(03) is expected, but was {0}", type.ToString("X2")));

            var length = ReadLength();
            var data = ReadBytes(length);
            return data;
        }

        /// <summary>
        /// Reads next object data type from internal buffer.
        /// </summary>
        /// <returns>data read.</returns>
        public byte[] ReadObject()
        {
            var type = ReadByte();
            if (type != Objectidentifier)
                throw new InvalidOperationException(string.Format("Invalid data type, OBJECT(06) is expected, but was {0}", type.ToString("X2")));

            var length = ReadLength();
            var data = ReadBytes(length);
            return data;
        }

        /// <summary>
        /// Writes BOOLEAN data into internal buffer.
        /// </summary>
        /// <param name="data">UInt32 data to write.</param>
        public void Write(bool data)
        {
            _data.Add(Boolean);
            _data.Add(1);
            _data.Add((byte)(data ? 1 : 0));
        }

        /// <summary>
        /// Writes UInt32 data into internal buffer.
        /// </summary>
        /// <param name="data">UInt32 data to write.</param>
        public void Write(uint data)
        {
            var bytes = Pack.UInt32ToBigEndian(data);
            _data.Add(Integer);
            var length = GetLength(bytes.Length);
            WriteBytes(length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes INTEGER data into internal buffer.
        /// </summary>
        /// <param name="data">BigInteger data to write.</param>
        public void Write(BigInteger data)
        {
            var bytes = data.ToByteArray().Reverse();
            _data.Add(Integer);
            var length = GetLength(bytes.Length);
            WriteBytes(length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes OCTETSTRING data into internal buffer.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Write(byte[] data)
        {
            _data.Add(Octetstring);
            var length = GetLength(data.Length);
            WriteBytes(length);
            WriteBytes(data);
        }

        /// <summary>
        /// Writes BITSTRING data into internal buffer.
        /// </summary>
        /// <param name="data">The data.</param>
        public void WriteBitstring(byte[] data)
        {
            _data.Add(BITSTRING);
            var length = GetLength(data.Length);
            WriteBytes(length);
            WriteBytes(data);
        }

        /// <summary>
        /// Writes OBJECTIDENTIFIER data into internal buffer.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        public void Write(ObjectIdentifier identifier)
        {
            var temp = new ulong[identifier.Identifiers.Length - 1];
            temp[0] = identifier.Identifiers[0] * 40 + identifier.Identifiers[1];
            Buffer.BlockCopy(identifier.Identifiers, 2 * sizeof(ulong), temp, 1 * sizeof(ulong), (identifier.Identifiers.Length - 2) * sizeof(ulong));
            var bytes = new List<byte>();
            foreach (var subidentifier in temp)
            {
                var item = subidentifier;
                var buffer = new byte[8];
                var bufferIndex = buffer.Length - 1;

                var current = (byte)(item & 0x7F);
                do
                {
                    buffer[bufferIndex] = current;
                    if (bufferIndex < buffer.Length - 1)
                        buffer[bufferIndex] |= 0x80;
                    item >>= 7;
                    current = (byte)(item & 0x7F);
                    bufferIndex--;
                }
                while (current > 0);

                for (var i = bufferIndex + 1; i < buffer.Length; i++)
                {
                    bytes.Add(buffer[i]);
                }
            }

            _data.Add(Objectidentifier);
            var length = GetLength(bytes.Count);
            WriteBytes(length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes OBJECTIDENTIFIER data into internal buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public void WriteObjectIdentifier(byte[] bytes)
        {
            _data.Add(Objectidentifier);
            var length = GetLength(bytes.Length);
            WriteBytes(length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes NULL data into internal buffer.
        /// </summary>
        public void WriteNull()
        {
            _data.Add(Null);
            _data.Add(0);
        }

        /// <summary>
        /// Writes DerData data into internal buffer.
        /// </summary>
        /// <param name="data">DerData data to write.</param>
        public void Write(DerData data)
        {
            var bytes = data.Encode();
            _data.AddRange(bytes);
        }

        private static IEnumerable<byte> GetLength(int length)
        {
            if (length > 127)
            {
                var size = 1;
                var val = length;

                while ((val >>= 8) != 0)
                    size++;

                var data = new byte[size];
                data[0] = (byte)(size | 0x80);

                for (int i = (size - 1) * 8, j = 1; i >= 0; i -= 8, j++)
                {
                    data[j] = (byte)(length >> i);
                }

                return data;
            }
            return new[] { (byte)length };
        }
        /// <summary>
        /// Gets Data Length
        /// </summary>
        /// <returns>length</returns>
        public int ReadLength()
        {
            int length = ReadByte();

            if (length == 0x80)
            {
                throw new NotSupportedException("Indefinite-length encoding is not supported.");
            }

            if (length > 127)
            {
                var size = length & 0x7f;

                // Note: The invalid long form "0xff" (see X.690 8.1.3.5c) will be caught here
                if (size > 4)
                    throw new InvalidOperationException(string.Format("DER length is '{0}' and cannot be more than 4 bytes.", size));

                length = 0;
                for (var i = 0; i < size; i++)
                {
                    int next = ReadByte();

                    length = (length << 8) + next;
                }

                if (length < 0)
                    throw new InvalidOperationException("Corrupted data - negative length found");

                //if (length >= limit)   // after all we must have read at least 1 byte
                //    throw new IOException("Corrupted stream - out of bounds length found");
            }

            return length;
        }

        /// <summary>
        /// Write Byte data into internal buffer.
        /// </summary>
        public void WriteBytes(IEnumerable<byte> data)
        {
            _data.AddRange(data);
        }

        /// <summary>
        /// Reads Byte data into internal buffer.
        /// </summary>
        /// <returns>data read</returns>
        public byte ReadByte()
        {
            if (_readerIndex > _data.Count)
                throw new InvalidOperationException("Read out of boundaries.");

            return _data[_readerIndex++];
        }

        /// <summary>
        /// Reads lengths Bytes data into internal buffer.
        /// </summary>
        /// <returns>data read</returns>
        ///  <param name="length">amount of data to read.</param>
        public byte[] ReadBytes(int length)
        {
            if (_readerIndex + length > _data.Count)
                throw new InvalidOperationException("Read out of boundaries.");

            var result = new byte[length];
            _data.CopyTo(_readerIndex, result, 0, length);
            _readerIndex += length;
            return result;
        }
    }
}