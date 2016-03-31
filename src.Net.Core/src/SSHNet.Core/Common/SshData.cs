using System;
using System.Collections.Generic;
#if false //old  !TUNING
using System.Linq;
#endif
using System.Text;
using System.Globalization;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base ssh data serialization type
    /// </summary>
    public abstract class SshData
    {
        internal const int DefaultCapacity = 64;

        internal static readonly Encoding Ascii = new ASCIIEncoding();

#if SILVERLIGHT
        internal static readonly Encoding Utf8 = Encoding.UTF8;
#else
        internal static readonly Encoding Utf8 = Encoding.GetEncoding(0);
#endif

#if true //old TUNING
        private SshDataStream _stream;

        protected SshDataStream DataStream
        {
            get { return _stream; }
        }
#else
        /// <summary>
        /// Data byte array that hold message unencrypted data
        /// </summary>
        private List<byte> _data;

        private int _readerIndex;
#endif

        /// <summary>
        /// Gets a value indicating whether all data from the buffer has been read.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is end of data; otherwise, <c>false</c>.
        /// </value>
        protected bool IsEndOfData
        {
            get
            {
#if true //old TUNING
                return _stream.Position >= _stream.Length;
#else
                return _readerIndex >= _data.Count();
#endif
            }
        }

        private byte[] _loadedData;
#if true //old TUNING
        private int _offset;
#endif

        /// <summary>
        /// Gets the index that represents zero in current data type.
        /// </summary>
        /// <value>
        /// The index of the zero reader.
        /// </value>
        protected virtual int ZeroReaderIndex
        {
            get
            {
                return 0;
            }
        }

#if true //old TUNING
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
#endif

        /// <summary>
        /// Gets data bytes array
        /// </summary>
        /// <returns>Byte array representation of data structure.</returns>
        public
#if false //old  !TUNING
        virtual
#endif
        byte[] GetBytes()
        {
#if true //old TUNING
            var messageLength = BufferCapacity;
            var capacity = messageLength != -1 ? messageLength : DefaultCapacity;
            var dataStream = new SshDataStream(capacity);
            WriteBytes(dataStream);
            return dataStream.ToArray();
#else
            _data = new List<byte>();

            SaveData();

            return _data.ToArray();
#endif
        }

#if true //old TUNING
        /// <summary>
        /// Writes the current message to the specified <see cref="SshDataStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="SshDataStream"/> to write the message to.</param>
        protected virtual void WriteBytes(SshDataStream stream)
        {
            _stream = stream;
            SaveData();
        }
#endif

        internal T OfType<T>() where T : SshData, new()
        {
            var result = new T();
#if true //old TUNING
            result.LoadBytes(_loadedData, _offset);
#else
            result.LoadBytes(_loadedData);
#endif
            result.LoadData();
            return result;
        }

        /// <summary>
        /// Loads data from specified bytes.
        /// </summary>
        /// <param name="value">Bytes array.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public void Load(byte[] value)
        {
#if true //old TUNING
            Load(value, 0);
#else
            if (value == null)
                throw new ArgumentNullException("value");

            LoadBytes(value);
            LoadData();
#endif
        }

#if true //old TUNING
        /// <summary>
        /// Loads data from the specified buffer.
        /// </summary>
        /// <param name="value">Bytes array.</param>
        /// <param name="offset">The zero-based offset in <paramref name="value"/> at which to begin reading SSH data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public void Load(byte[] value, int offset)
        {
            LoadBytes(value, offset);
            LoadData();
        }
#endif

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected abstract void SaveData();

        /// <summary>
        /// Loads data bytes into internal buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
        protected void LoadBytes(byte[] bytes)
        {
#if true //old TUNING
            LoadBytes(bytes, 0);
#else
            // Note about why I check for null here, and in Load(byte[]) in this class.
            // This method is called by several other classes, such as SshNet.Messages.Message, SshNet.Sftp.SftpMessage.
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            ResetReader();
            _loadedData = bytes;
            _data = new List<byte>(bytes);
#endif
        }

#if true //old TUNING
        /// <summary>
        /// Loads data bytes into internal buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The zero-based offset in <paramref name="bytes"/> at which to begin reading SSH data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
        protected void LoadBytes(byte[] bytes, int offset)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            _loadedData = bytes;
            _offset = offset;

            _stream = new SshDataStream(bytes);
            ResetReader();
        }
#endif

        /// <summary>
        /// Resets internal data reader index.
        /// </summary>
        protected void ResetReader()
        {
#if true //old TUNING
            _stream.Position = ZeroReaderIndex + _offset;
#else
            _readerIndex = ZeroReaderIndex;  //  Set to 1 to skip first byte which specifies message type
#endif
        }

        /// <summary>
        /// Reads all data left in internal buffer at current position.
        /// </summary>
        /// <returns>An array of bytes containing the remaining data in the internal buffer.</returns>
        protected byte[] ReadBytes()
        {
#if true //old TUNING
            var bytesLength = (int) (_stream.Length - _stream.Position);
            var data = new byte[bytesLength];
            _stream.Read(data, 0, bytesLength);
            return data;
#else
            var data = new byte[_data.Count - _readerIndex];
            _data.CopyTo(_readerIndex, data, 0, data.Length);
            return data;
#endif
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

#if true //old TUNING
            var data = new byte[length];
            var bytesRead = _stream.Read(data, 0, length);

            if (bytesRead < length)
                throw new ArgumentOutOfRangeException("length");

            return data;
#else
            if (length > _data.Count)
                throw new ArgumentOutOfRangeException("length");

            var result = new byte[length];
            _data.CopyTo(_readerIndex, result, 0, length);
            _readerIndex += length;
            return result;
#endif
        }

        /// <summary>
        /// Reads next byte data type from internal buffer.
        /// </summary>
        /// <returns>Byte read.</returns>
        protected byte ReadByte()
        {
#if true //old TUNING
            var byteRead = _stream.ReadByte();
            if (byteRead == -1)
                throw new InvalidOperationException("Attempt to read past the end of the SSH data stream.");
            return (byte) byteRead;
#else
            return ReadBytes(1).FirstOrDefault();
#endif
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
            var data = ReadBytes(2);
            return (ushort)(data[0] << 8 | data[1]);
        }

        /// <summary>
        /// Reads next uint32 data type from internal buffer.
        /// </summary>
        /// <returns>uint32 read</returns>
        protected uint ReadUInt32()
        {
            var data = ReadBytes(4);
            return (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
        }

        /// <summary>
        /// Reads next uint64 data type from internal buffer.
        /// </summary>
        /// <returns>uint64 read</returns>
        protected ulong ReadUInt64()
        {
            var data = ReadBytes(8);
            return ((ulong)data[0] << 56 | (ulong)data[1] << 48 | (ulong)data[2] << 40 | (ulong)data[3] << 32 | (ulong)data[4] << 24 | (ulong)data[5] << 16 | (ulong)data[6] << 8 | data[7]);
        }

#if false //old  !TUNING
        /// <summary>
        /// Reads next string data type from internal buffer.
        /// </summary>
        /// <returns>string read</returns>
        protected string ReadAsciiString()
        {
            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }
            return Ascii.GetString(ReadBytes((int)length), 0, (int)length);
        }
#endif

        /// <summary>
        /// Reads next string data type from internal buffer.
        /// </summary>
        /// <returns>string read</returns>
        protected string ReadString()
        {
            return ReadString(Utf8);
        }

        /// <summary>
        /// Reads next string data type from internal buffer.
        /// </summary>
        /// <returns>string read</returns>
        protected string ReadString(Encoding encoding)
        {
#if true //old TUNING
            return _stream.ReadString(encoding);
#else
            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }
            return encoding.GetString(ReadBytes((int)length), 0, (int)length);
#endif
        }

#if true //old TUNING
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
#else
        /// <summary>
        /// Reads next string data type from internal buffer.
        /// </summary>
        /// <returns>string read</returns>
        protected byte[] ReadBinaryString()
        {
            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }

            return ReadBytes((int)length);
        }
#endif

        /// <summary>
        /// Reads next name-list data type from internal buffer.
        /// </summary>
        /// <returns>String array or read data..</returns>
        protected string[] ReadNamesList()
        {
            var namesList = ReadString();
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
                var extensionName = ReadString();
                var extensionData = ReadString();
                result.Add(extensionName, extensionData);
            }
            return result;
        }

#if true //old TUNING
        /// <summary>
        /// Writes bytes array data into internal buffer.
        /// </summary>
        /// <param name="data">Byte array data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void Write(byte[] data)
        {
            _stream.Write(data);
        }
#else
        /// <summary>
        /// Writes bytes array data into internal buffer.
        /// </summary>
        /// <param name="data">Byte array data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void Write(IEnumerable<byte> data)
        {
            _data.AddRange(data);
        }
#endif

#if true //old TUNING
        /// <summary>
        /// Writes a sequence of bytes to the current SSH data stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
        /// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        protected void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
#endif

        /// <summary>
        /// Writes byte data into internal buffer.
        /// </summary>
        /// <param name="data">Byte data to write.</param>
        protected void Write(byte data)
        {
#if true //old TUNING
            _stream.WriteByte(data);
#else
            _data.Add(data);
#endif
        }

        /// <summary>
        /// Writes boolean data into internal buffer.
        /// </summary>
        /// <param name="data">Boolean data to write.</param>
        protected void Write(bool data)
        {
            Write(data ? (byte) 1 : (byte) 0);
        }

        /// <summary>
        /// Writes uint32 data into internal buffer.
        /// </summary>
        /// <param name="data">uint32 data to write.</param>
        protected void Write(uint data)
        {
#if true //old TUNING
            _stream.Write(data);
#else
            Write(data.GetBytes());
#endif
        }

        /// <summary>
        /// Writes uint64 data into internal buffer.
        /// </summary>
        /// <param name="data">uint64 data to write.</param>
        protected void Write(ulong data)
        {
#if true //old TUNING
            _stream.Write(data);
#else
            Write(data.GetBytes());
#endif
        }

        /// <summary>
        /// Writes string data into internal buffer using default encoding.
        /// </summary>
        /// <param name="data">string data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void Write(string data)
        {
            Write(data, Utf8);
        }

        /// <summary>
        /// Writes string data into internal buffer using the specified encoding.
        /// </summary>
        /// <param name="data">string data to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        protected void Write(string data, Encoding encoding)
        {
#if true //old TUNING
            _stream.Write(data, encoding);
#else
            if (data == null)
                throw new ArgumentNullException("data");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            var bytes = encoding.GetBytes(data);

            Write((uint)bytes.Length);
            Write(bytes);
#endif
        }

#if true //old TUNING
        /// <summary>
        /// Writes data into internal buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        protected void WriteBinary(byte[] buffer, int offset, int count)
        {
            _stream.WriteBinary(buffer, offset, count);
        }
#else
        /// <summary>
        /// Writes string data into internal buffer.
        /// </summary>
        /// <param name="data">string data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void WriteBinaryString(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Write((uint)data.Length);
            _data.AddRange(data);
        }
#endif

        /// <summary>
        /// Writes mpint data into internal buffer.
        /// </summary>
        /// <param name="data">mpint data to write.</param>
        protected void Write(BigInteger data)
        {
#if true //old TUNING
            _stream.Write(data);
#else
            var bytes = data.ToByteArray().Reverse().ToList();
            Write((uint)bytes.Count);
            Write(bytes);
#endif
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
