using System;
using System.Collections.Generic;
using System.Linq;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for DER encoded data.
    /// </summary>
    public class DerData
    {
        private const byte CONSTRUCTED = 0x20;

        private const byte BOOLEAN = 0x01;
        private const byte INTEGER = 0x02;
        //private const byte BITSTRING = 0x03;
        private const byte OCTETSTRING = 0x04;
        private const byte NULL = 0x05;
        private const byte OBJECTIDENTIFIER = 0x06;
        //private const byte EXTERNAL = 0x08;
        //private const byte ENUMERATED = 0x0a;
        private const byte SEQUENCE = 0x10;
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
                return this._readerIndex >= this._lastIndex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerData"/> class.
        /// </summary>
        public DerData()
        {
            this._data = new List<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerData"/> class.
        /// </summary>
        /// <param name="data">DER encoded data.</param>
        public DerData(byte[] data)
        {
            this._data = new List<byte>(data);
            var dataType = this.ReadByte();
            var length = this.ReadLength();
            this._lastIndex = this._readerIndex + length;
        }

        /// <summary>
        /// Encodes written data as DER byte array.
        /// </summary>
        /// <returns>DER Encoded array.</returns>
        public byte[] Encode()
        {
            var length = this._data.Count();
            var lengthBytes = this.GetLength(length);

            this._data.InsertRange(0, lengthBytes);
            this._data.Insert(0, CONSTRUCTED | SEQUENCE);

            return this._data.ToArray();
        }

        /// <summary>
        /// Reads next mpint data type from internal buffer.
        /// </summary>
        /// <returns>mpint read.</returns>
        public BigInteger ReadBigInteger()
        {
            var type = this.ReadByte();
            if (type != INTEGER)
                throw new InvalidOperationException("Invalid data type, INTEGER(02) is expected.");

            var length = this.ReadLength();

            var data = this.ReadBytes(length);

            return new BigInteger(data.Reverse().ToArray());
        }

        /// <summary>
        /// Reads next int data type from internal buffer.
        /// </summary>
        /// <returns>int read.</returns>
        public int ReadInteger()
        {
            var type = this.ReadByte();
            if (type != INTEGER)
                throw new InvalidOperationException("Invalid data type, INTEGER(02) is expected.");

            var length = this.ReadLength();

            var data = this.ReadBytes(length);

            if (length > 4)
                throw new InvalidOperationException("Integer type cannot occupy more then 4 bytes");

            var result = 0;
            var shift = (length - 1) * 8;
            for (int i = 0; i < length; i++)
            {
                result |= data[i] << shift;
                shift -= 8;
            }

            //return (int)(data[0] << 56 | data[1] << 48 | data[2] << 40 | data[3] << 32 | data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7]);

            return result;
        }

        /// <summary>
        /// Writes BOOLEAN data into internal buffer.
        /// </summary>
        /// <param name="data">UInt32 data to write.</param>
        public void Write(bool data)
        {
            this._data.Add(BOOLEAN);
            this._data.Add(1);
            this._data.Add((byte)(data ? 1 : 0));
        }

        /// <summary>
        /// Writes UInt32 data into internal buffer.
        /// </summary>
        /// <param name="data">UInt32 data to write.</param>
        public void Write(UInt32 data)
        {
            var bytes = data.GetBytes();
            this._data.Add(INTEGER);
            var length = this.GetLength(bytes.Length);
            this.WriteBytes(length);
            this.WriteBytes(bytes);
        }

        /// <summary>
        /// Writes INTEGER data into internal buffer.
        /// </summary>
        /// <param name="data">BigInteger data to write.</param>
        public void Write(BigInteger data)
        {
            var bytes = data.ToByteArray().Reverse().ToList();
            this._data.Add(INTEGER);
            var length = this.GetLength(bytes.Count);
            this.WriteBytes(length);
            this.WriteBytes(bytes);
        }

        /// <summary>
        /// Writes OCTETSTRING data into internal buffer.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Write(byte[] data)
        {
            this._data.Add(OCTETSTRING);
            var length = this.GetLength(data.Length);
            this.WriteBytes(length);
            this.WriteBytes(data);
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
            //Array.Copy(identifier.Identifiers, 2, temp, 1, identifier.Identifiers.Length - 2);
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
                        buffer[bufferIndex] |= (byte)0x80;
                    item >>= 7;
                    current = (byte)(item & 0x7F);
                    bufferIndex--;
                }
                while (current > 0);

                for (int i = bufferIndex + 1; i < buffer.Length; i++)
                {
                    bytes.Add(buffer[i]);
                }
            }

            this._data.Add(OBJECTIDENTIFIER);
            var length = this.GetLength(bytes.Count);
            this.WriteBytes(length);
            this.WriteBytes(bytes);
        }

        /// <summary>
        /// Writes NULL data into internal buffer.
        /// </summary>
        public void WriteNull()
        {
            this._data.Add(NULL);
            this._data.Add(0);
        }

        /// <summary>
        /// Writes DerData data into internal buffer.
        /// </summary>
        /// <param name="data">DerData data to write.</param>
        public void Write(DerData data)
        {
            var bytes = data.Encode();
            this._data.AddRange(bytes);
        }

        private byte[] GetLength(int length)
        {
            if (length > 127)
            {
                int size = 1;
                int val = length;

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
            return new byte[] { (byte)length };
        }

        private int ReadLength()
        {
            int length = this.ReadByte();

            if (length == 0x80)
            {
                throw new NotSupportedException("Indefinite-length encoding is not supported.");
            }

            if (length > 127)
            {
                int size = length & 0x7f;

                // Note: The invalid long form "0xff" (see X.690 8.1.3.5c) will be caught here
                if (size > 4)
                    throw new InvalidOperationException(string.Format("DER length is '{0}' and cannot be more than 4 bytes.", size));

                length = 0;
                for (int i = 0; i < size; i++)
                {
                    int next = this.ReadByte();

                    length = (length << 8) + next;
                }

                if (length < 0)
                    throw new InvalidOperationException("Corrupted data - negative length found");

                //if (length >= limit)   // after all we must have read at least 1 byte
                //    throw new IOException("Corrupted stream - out of bounds length found");
            }

            return length;
        }

        private void WriteBytes(IEnumerable<byte> data)
        {
            this._data.AddRange(data);
        }

        private byte ReadByte()
        {
            if (this._readerIndex > this._data.Count)
                throw new InvalidOperationException("Read out of boundaries.");

            return this._data[this._readerIndex++];
        }

        private byte[] ReadBytes(int length)
        {
            if (this._readerIndex + length > this._data.Count)
                throw new InvalidOperationException("Read out of boundaries.");

            var result = new byte[length];
            this._data.CopyTo(this._readerIndex, result, 0, length);
            this._readerIndex += length;
            return result;
        }
    }
}
