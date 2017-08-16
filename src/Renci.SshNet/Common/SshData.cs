using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base ssh data serialization type
    /// </summary>
    public abstract class SshData
    {
        internal const int DefaultCapacity = 64;

#if FEATURE_ENCODING_ASCII
        internal static readonly Encoding Ascii = Encoding.ASCII;
#else
        internal static readonly Encoding Ascii = new ASCIIEncoding();
#endif
        internal static readonly Encoding Utf8 = Encoding.UTF8;

        private SshDataStream _stream;

        /// <summary>
        /// Gets the underlying <see cref="SshDataStream"/> that is used for reading and writing SSH data.
        /// </summary>
        /// <value>
        /// The underlying <see cref="SshDataStream"/> that is used for reading and writing SSH data.
        /// </value>
        protected SshDataStream DataStream
        {
            get { return _stream; }
        }

        /// <summary>
        /// Gets a value indicating whether all data from the buffer has been read.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is end of data; otherwise, <c>false</c>.
        /// </value>
        protected bool IsEndOfData
        {
            get
            {
                return _stream.Position >= _stream.Length;
            }
        }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected virtual int BufferCapacity
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets data bytes array.
        /// </summary>
        /// <returns>
        /// A <see cref="Byte"/> array representation of data structure.
        /// </returns>
        public byte[] GetBytes()
        {
            var messageLength = BufferCapacity;
            var capacity = messageLength != -1 ? messageLength : DefaultCapacity;
            var dataStream = new SshDataStream(capacity);
            WriteBytes(dataStream);
            return dataStream.ToArray();
        }

        /// <summary>
        /// Writes the current message to the specified <see cref="SshDataStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="SshDataStream"/> to write the message to.</param>
        protected virtual void WriteBytes(SshDataStream stream)
        {
            _stream = stream;
            SaveData();
        }

        /// <summary>
        /// Loads data from specified bytes.
        /// </summary>
        /// <param name="data">Bytes array.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public void Load(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            LoadInternal(data, 0, data.Length);
        }

        /// <summary>
        /// Loads data from the specified buffer.
        /// </summary>
        /// <param name="data">Bytes array.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin reading SSH data.</param>
        /// <param name="count">The number of bytes to load.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public void Load(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            LoadInternal(data, offset, count);
        }

        private void LoadInternal(byte[] value, int offset, int count)
        {
            _stream = new SshDataStream(value, offset, count);
            LoadData();
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
        /// Reads all data left in internal buffer at current position.
        /// </summary>
        /// <returns>An array of bytes containing the remaining data in the internal buffer.</returns>
        protected byte[] ReadBytes()
        {
            var bytesLength = (int) (_stream.Length - _stream.Position);
            var data = new byte[bytesLength];
            _stream.Read(data, 0, bytesLength);
            return data;
        }

        /// <summary>
        /// Reads next specified number of bytes data type from internal buffer.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>An array of bytes that was read from the internal buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is greater than the internal buffer size.</exception>
        protected byte[] ReadBytes(int length)
        {
            // Note that this also prevents allocating non-relevant lengths, such as if length is greater than _data.Count but less than int.MaxValue.
            // For the nerds, the condition translates to: if (length > data.Count && length < int.MaxValue)
            // Which probably would cause all sorts of exception, most notably OutOfMemoryException.

            var data = new byte[length];
            var bytesRead = _stream.Read(data, 0, length);

            if (bytesRead < length)
                throw new ArgumentOutOfRangeException("length");

            return data;
        }

        /// <summary>
        /// Reads next byte data type from internal buffer.
        /// </summary>
        /// <returns>Byte read.</returns>
        protected byte ReadByte()
        {
            var byteRead = _stream.ReadByte();
            if (byteRead == -1)
                throw new InvalidOperationException("Attempt to read past the end of the SSH data stream.");
            return (byte) byteRead;
        }

        /// <summary>
        /// Reads next boolean data type from internal buffer.
        /// </summary>
        /// <returns>Boolean read.</returns>
        protected bool ReadBoolean()
        {
            return ReadByte() != 0;
        }

        /// <summary>
        /// Reads next uint16 data type from internal buffer.
        /// </summary>
        /// <returns>uint16 read</returns>
        protected ushort ReadUInt16()
        {
            return Pack.BigEndianToUInt16(ReadBytes(2));
        }

        /// <summary>
        /// Reads next uint32 data type from internal buffer.
        /// </summary>
        /// <returns>uint32 read</returns>
        protected uint ReadUInt32()
        {
            return Pack.BigEndianToUInt32(ReadBytes(4));
        }

        /// <summary>
        /// Reads next uint64 data type from internal buffer.
        /// </summary>
        /// <returns>uint64 read</returns>
        protected ulong ReadUInt64()
        {
            return Pack.BigEndianToUInt64(ReadBytes(8));
        }

        /// <summary>
        /// Reads next string data type from internal buffer using the specific encoding.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> read.
        /// </returns>
        protected string ReadString(Encoding encoding)
        {
            return _stream.ReadString(encoding);
        }

        /// <summary>
        /// Reads next data type as byte array from internal buffer.
        /// </summary>
        /// <returns>
        /// The bytes read.
        /// </returns>
        protected byte[] ReadBinary()
        {
            return _stream.ReadBinary();
        }

        /// <summary>
        /// Reads next name-list data type from internal buffer.
        /// </summary>
        /// <returns>
        /// String array or read data.
        /// </returns>
        protected string[] ReadNamesList()
        {
            var namesList = ReadString(Ascii);
            return namesList.Split(',');
        }

        /// <summary>
        /// Reads next extension-pair data type from internal buffer.
        /// </summary>
        /// <returns>Extensions pair dictionary.</returns>
        protected IDictionary<string, string> ReadExtensionPair()
        {
            var result = new Dictionary<string, string>();
            while (!IsEndOfData)
            {
                var extensionName = ReadString(Ascii);
                var extensionData = ReadString(Ascii);
                result.Add(extensionName, extensionData);
            }
            return result;
        }

        /// <summary>
        /// Writes bytes array data into internal buffer.
        /// </summary>
        /// <param name="data">Byte array data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected void Write(byte[] data)
        {
            _stream.Write(data);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current SSH data stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
        /// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        protected void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes <see cref="byte"/> data into internal buffer.
        /// </summary>
        /// <param name="data"><see cref="byte"/> data to write.</param>
        protected void Write(byte data)
        {
            _stream.WriteByte(data);
        }

        /// <summary>
        /// Writes <see cref="bool"/> into internal buffer.
        /// </summary>
        /// <param name="data"><see cref="bool" /> data to write.</param>
        protected void Write(bool data)
        {
            Write(data ? (byte) 1 : (byte) 0);
        }

        /// <summary>
        /// Writes <see cref="uint"/> data into internal buffer.
        /// </summary>
        /// <param name="data"><see cref="uint"/> data to write.</param>
        protected void Write(uint data)
        {
            _stream.Write(data);
        }

        /// <summary>
        /// Writes <see cref="ulong" /> data into internal buffer.
        /// </summary>
        /// <param name="data"><see cref="ulong"/> data to write.</param>
        protected void Write(ulong data)
        {
            _stream.Write(data);
        }

        /// <summary>
        /// Writes <see cref="string"/> data into internal buffer using default encoding.
        /// </summary>
        /// <param name="data"><see cref="string"/> data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected void Write(string data)
        {
            Write(data, Utf8);
        }

        /// <summary>
        /// Writes <see cref="string"/> data into internal buffer using the specified encoding.
        /// </summary>
        /// <param name="data"><see cref="string"/> data to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
        protected void Write(string data, Encoding encoding)
        {
            _stream.Write(data, encoding);
        }

        /// <summary>
        /// Writes data into internal buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        protected void WriteBinaryString(byte[] buffer)
        {
            _stream.WriteBinary(buffer);
        }

        /// <summary>
        /// Writes data into internal buffer.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
        /// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        protected void WriteBinary(byte[] buffer, int offset, int count)
        {
            _stream.WriteBinary(buffer, offset, count);
        }

        /// <summary>
        /// Writes mpint data into internal buffer.
        /// </summary>
        /// <param name="data">mpint data to write.</param>
        protected void Write(BigInteger data)
        {
            _stream.Write(data);
        }

        /// <summary>
        /// Writes name-list data into internal buffer.
        /// </summary>
        /// <param name="data">name-list data to write.</param>
        protected void Write(string[] data)
        {
            Write(string.Join(",", data), Ascii);
        }

        /// <summary>
        /// Writes extension-pair data into internal buffer.
        /// </summary>
        /// <param name="data">extension-pair data to write.</param>
        protected void Write(IDictionary<string, string> data)
        {
            foreach (var item in data)
            {
                Write(item.Key, Ascii);
                Write(item.Value, Ascii);
            }
        }
    }
}
