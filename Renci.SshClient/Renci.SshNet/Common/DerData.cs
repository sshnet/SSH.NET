using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for DER encoded data.
    /// </summary>
    public abstract class DerData
    {
        private const byte CONSTRUCTED = 0x20;

        private const byte BOOLEAN = 0x01;
        private const byte INTEGER = 0x02;
        private const byte BITSTRING = 0x03;
        private const byte OCTETSTRING = 0x04;
        private const byte NULL = 0x05;
        private const byte OBJECTIDENTIFIER = 0x06;
        private const byte EXTERNAL = 0x08;
        private const byte ENUMERATED = 0x0a;
        private const byte SEQUENCE = 0x10;
        private const byte SEQUENCEOF = 0x10; // for completeness
        private const byte SET = 0x11;
        private const byte SETOF = 0x11; // for completeness

        private const byte NUMERICSTRING = 0x12;
        private const byte PRINTABLESTRING = 0x13;
        private const byte T61STRING = 0x14;
        private const byte VIDEOTEXSTRING = 0x15;
        private const byte IA5STRING = 0x16;
        private const byte UTCTIME = 0x17;
        private const byte GENERALIZEDTIME = 0x18;
        private const byte GRAPHICSTRING = 0x19;
        private const byte VISIBLESTRING = 0x1a;
        private const byte GENERALSTRING = 0x1b;
        private const byte UNIVERSALSTRING = 0x1c;
        private const byte BMPSTRING = 0x1e;
        private const byte UTF8STRING = 0x0c;
        private const byte APPLICATION = 0x40;
        private const byte TAGGED = 0x80;

        private List<byte> _data;

        private int _readerIndex = 0;

        public byte[] Encode()
        {
            this._data = new List<byte>();

            this.SaveData();

            var length = this._data.Count();
            var lengthBytes = this.GetLength(length);

            this._data.InsertRange(0, lengthBytes);
            this._data.Insert(0, CONSTRUCTED | SEQUENCE);

            return this._data.ToArray();
        }

        public void Decode(byte[] data)
        {
            this._data = new List<byte>(data);
            this._readerIndex = 0;
            var dataType = this.ReadByte();
            var length = this.ReadLength();

            this.LoadData();
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected abstract void SaveData();

        /// <summary>
        /// Reads next mpint data type from internal buffer.
        /// </summary>
        /// <returns>mpint read.</returns>
        protected BigInteger ReadBigInt()
        {
            var type = this.ReadByte();
            if (type != INTEGER)
                throw new InvalidOperationException("Invalid data type, INTEGER(02) is expected.");

            var length = this.ReadLength();

            var data = this.ReadBytes(length);

            return new BigInteger(data.Reverse().ToArray());
        }

        /// <summary>
        /// Writes uint32 data into internal buffer.
        /// </summary>
        /// <param name="data">uint32 data to write.</param>
        protected void Write(UInt32 data)
        {
            var bytes = data.GetBytes();
            this._data.Add(INTEGER);
            var length = this.GetLength(bytes.Length);
            this.WriteBytes(length);
            this.WriteBytes(bytes);
        }

        protected void Write(BigInteger data)
        {
            var bytes = data.ToByteArray().Reverse().ToList();
            this._data.Add(INTEGER);
            var length = this.GetLength(bytes.Count);
            this.WriteBytes(length);
            this.WriteBytes(bytes);
        }

        protected void Write(DerData data)
        {
            throw new NotImplementedException();
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
            else
            {
                return new byte[] { (byte)length };
            }
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
