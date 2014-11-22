using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base ssh data serialization type
    /// </summary>
    public abstract class SshData
    {
        private static readonly Encoding Ascii = new ASCIIEncoding();

#if SILVERLIGHT
        private static readonly Encoding Utf8 = Encoding.UTF8;
#else
        private static readonly Encoding Utf8 = Encoding.Default;
#endif

        /// <summary>
        /// Data byte array that hold message unencrypted data
        /// </summary>
        private List<byte> _data;

        private int _readerIndex;

        /// <summary>
        /// Gets a value indicating whether all data from the buffer has been read.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is end of data; otherwise, <c>false</c>.
        /// </value>
        public bool IsEndOfData
        {
            get
            {
                return _readerIndex >= _data.Count();
            }
        }

        private byte[] _loadedData;

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

        /// <summary>
        /// Gets data bytes array
        /// </summary>
        /// <returns>Byte array representation of data structure.</returns>
        public virtual byte[] GetBytes()
        {
            _data = new List<byte>();

            SaveData();

            return _data.ToArray();
        }

        internal T OfType<T>() where T : SshData, new()
        {
            var result = new T();
            result.LoadBytes(_loadedData);
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
            if (value == null)
                throw new ArgumentNullException("value");

            LoadBytes(value);
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
        /// Loads data bytes into internal buffer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
        protected void LoadBytes(byte[] bytes)
        {
            // Note about why I check for null here, and in Load(byte[]) in this class.
            // This method is called by several other classes, such as SshNet.Messages.Message, SshNet.Sftp.SftpMessage.
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            ResetReader();
            _loadedData = bytes;
            _data = new List<byte>(bytes);
        }

        /// <summary>
        /// Resets internal data reader index.
        /// </summary>
        protected void ResetReader()
        {
            _readerIndex = ZeroReaderIndex;  //  Set to 1 to skip first byte which specifies message type
        }

        /// <summary>
        /// Reads all data left in internal buffer at current position.
        /// </summary>
        /// <returns>An array of bytes containing the remaining data in the internal buffer.</returns>
        protected byte[] ReadBytes()
        {
            var data = new byte[_data.Count - _readerIndex];
            _data.CopyTo(_readerIndex, data, 0, data.Length);
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
            if (length > _data.Count)
                throw new ArgumentOutOfRangeException("length");

            var result = new byte[length];
            _data.CopyTo(_readerIndex, result, 0, length);
            _readerIndex += length;
            return result;
        }

        /// <summary>
        /// Reads next byte data type from internal buffer.
        /// </summary>
        /// <returns>Byte read.</returns>
        protected byte ReadByte()
        {
            return ReadBytes(1).FirstOrDefault();
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
            return ((ulong) data[0] << 56 | (ulong) data[1] << 48 | (ulong) data[2] << 40 | (ulong) data[3] << 32 |
                    (ulong) data[4] << 24 | (ulong) data[5] << 16 | (ulong) data[6] << 8 | data[7]);
        }

        /// <summary>
        /// Reads next int64 data type from internal buffer.
        /// </summary>
        /// <returns>int64 read</returns>
        protected long ReadInt64()
        {
            var data = ReadBytes(8);
            return data[0] << 56 | data[1] << 48 | data[2] << 40 | data[3] << 32 | data[4] << 24 | data[5] << 16 |
                   data[6] << 8 | data[7];
        }

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
            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }
            return encoding.GetString(ReadBytes((int)length), 0, (int)length);
        }


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

        /// <summary>
        /// Reads next mpint data type from internal buffer.
        /// </summary>
        /// <returns>mpint read.</returns>
        protected BigInteger ReadBigInt()
        {
            var length = ReadUInt32();

            var data = ReadBytes((int)length);

            return new BigInteger(data.Reverse().ToArray());
        }

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
            while (_readerIndex < _data.Count)
            {
                var extensionName = ReadString();
                var extensionData = ReadString();
                result.Add(extensionName, extensionData);
            }
            return result;
        }

        /// <summary>
        /// Writes bytes array data into internal buffer.
        /// </summary>
        /// <param name="data">Byte array data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void Write(IEnumerable<byte> data)
        {
            _data.AddRange(data);
        }

        /// <summary>
        /// Writes byte data into internal buffer.
        /// </summary>
        /// <param name="data">Byte data to write.</param>
        protected void Write(byte data)
        {
            _data.Add(data);
        }

        /// <summary>
        /// Writes boolean data into internal buffer.
        /// </summary>
        /// <param name="data">Boolean data to write.</param>
        protected void Write(bool data)
        {
            Write(data ? 1 : 0);
        }

        /// <summary>
        /// Writes uint16 data into internal buffer.
        /// </summary>
        /// <param name="data">uint16 data to write.</param>
        protected void Write(UInt16 data)
        {
            Write(data.GetBytes());
        }

        /// <summary>
        /// Writes uint32 data into internal buffer.
        /// </summary>
        /// <param name="data">uint32 data to write.</param>
        protected void Write(UInt32 data)
        {
            Write(data.GetBytes());
        }

        /// <summary>
        /// Writes uint64 data into internal buffer.
        /// </summary>
        /// <param name="data">uint64 data to write.</param>
        protected void Write(UInt64 data)
        {
            Write(data.GetBytes());
        }

        /// <summary>
        /// Writes int64 data into internal buffer.
        /// </summary>
        /// <param name="data">int64 data to write.</param>
        protected void Write(Int64 data)
        {
            Write(data.GetBytes());
        }


        /// <summary>
        /// Writes string data into internal buffer as ASCII.
        /// </summary>
        /// <param name="data">string data to write.</param>
        protected void WriteAscii(string data)
        {
            Write(data, Ascii);
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
            if (data == null)
                throw new ArgumentNullException("data");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            var bytes = encoding.GetBytes(data);
            Write((uint)bytes.Length);
            Write(bytes);
        }

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

        /// <summary>
        /// Writes mpint data into internal buffer.
        /// </summary>
        /// <param name="data">mpint data to write.</param>
        protected void Write(BigInteger data)
        {
            var bytes = data.ToByteArray().Reverse().ToList();
            Write((uint)bytes.Count);
            Write(bytes);
        }

        /// <summary>
        /// Writes name-list data into internal buffer.
        /// </summary>
        /// <param name="data">name-list data to write.</param>
        protected void Write(string[] data)
        {
            WriteAscii(string.Join(",", data));
        }

        /// <summary>
        /// Writes extension-pair data into internal buffer.
        /// </summary>
        /// <param name="data">extension-pair data to write.</param>
        protected void Write(IDictionary<string, string> data)
        {
            foreach (var item in data)
            {
                WriteAscii(item.Key);
                WriteAscii(item.Value);
            }
        }
    }
}
