﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base ssh data serialization type.
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public abstract class SshData
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        internal const int DefaultCapacity = 64;

        internal static readonly Encoding Ascii = Encoding.ASCII;

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
        /// <see langword="true"/> if this instance is end of data; otherwise, <see langword="false"/>.
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
        /// A <see cref="byte"/> array representation of data structure.
        /// </returns>
        public byte[] GetBytes()
        {
            var messageLength = BufferCapacity;
            var capacity = messageLength != -1 ? messageLength : DefaultCapacity;

            using (var dataStream = new SshDataStream(capacity))
            {
                WriteBytes(dataStream);
                return dataStream.ToArray();
            }
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
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public void Load(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            LoadInternal(data, 0, data.Length);
        }

        /// <summary>
        /// Loads data from the specified buffer.
        /// </summary>
        /// <param name="data">Bytes array.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin reading SSH data.</param>
        /// <param name="count">The number of bytes to load.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public void Load(byte[] data, int offset, int count)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

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
        /// <returns>
        /// An array of bytes containing the remaining data in the internal buffer.
        /// </returns>
        protected byte[] ReadBytes()
        {
            var bytesLength = (int)(_stream.Length - _stream.Position);
            var data = new byte[bytesLength];
            _ = _stream.Read(data, 0, bytesLength);
            return data;
        }

        /// <summary>
        /// Reads next specified number of bytes data type from internal buffer.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>
        /// An array of bytes that was read from the internal buffer.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is greater than the number of bytes available to be read.</exception>
        protected byte[] ReadBytes(int length)
        {
            return _stream.ReadBytes(length);
        }

        /// <summary>
        /// Reads next byte data type from internal buffer.
        /// </summary>
        /// <returns>
        /// The <see cref="byte"/> read.
        /// </returns>
        /// <exception cref="InvalidOperationException">Attempt to read past the end of the stream.</exception>
        protected byte ReadByte()
        {
            var byteRead = _stream.ReadByte();
            if (byteRead == -1)
            {
                throw new InvalidOperationException("Attempt to read past the end of the SSH data stream.");
            }

            return (byte)byteRead;
        }

        /// <summary>
        /// Reads the next <see cref="bool"/> from the internal buffer.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/> that was read.
        /// </returns>
        /// <exception cref="InvalidOperationException">Attempt to read past the end of the stream.</exception>
        protected bool ReadBoolean()
        {
            return ReadByte() != 0;
        }

        /// <summary>
        /// Reads the next <see cref="ushort"/> from the internal buffer.
        /// </summary>
        /// <returns>
        /// The <see cref="ushort"/> that was read.
        /// </returns>
        /// <exception cref="InvalidOperationException">Attempt to read past the end of the stream.</exception>
        protected ushort ReadUInt16()
        {
            return _stream.ReadUInt16();
        }

        /// <summary>
        /// Reads the next <see cref="uint"/> from the internal buffer.
        /// </summary>
        /// <returns>
        /// The <see cref="uint"/> that was read.
        /// </returns>
        /// <exception cref="InvalidOperationException">Attempt to read past the end of the stream.</exception>
        protected uint ReadUInt32()
        {
            return _stream.ReadUInt32();
        }

        /// <summary>
        /// Reads the next <see cref="ulong"/> from the internal buffer.
        /// </summary>
        /// <returns>
        /// The <see cref="ulong"/> that was read.
        /// </returns>
        /// <exception cref="InvalidOperationException">Attempt to read past the end of the stream.</exception>
        protected ulong ReadUInt64()
        {
            return _stream.ReadUInt64();
        }

        /// <summary>
        /// Reads the next <see cref="string"/> from the internal buffer using the specified encoding.
        /// </summary>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>
        /// The <see cref="string"/> that was read.
        /// </returns>
        protected string ReadString(Encoding encoding = null)
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
        /// <returns>
        /// Extensions pair dictionary.
        /// </returns>
        protected Dictionary<string, string> ReadExtensionPair()
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
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
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
            Write(data ? (byte)1 : (byte)0);
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
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        protected void Write(string data)
        {
            Write(data, Utf8);
        }

        /// <summary>
        /// Writes <see cref="string"/> data into internal buffer using the specified encoding.
        /// </summary>
        /// <param name="data"><see cref="string"/> data to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
        protected void Write(string data, Encoding encoding)
        {
            _stream.Write(data, encoding);
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
#if NET || NETSTANDARD2_1_OR_GREATER
            Write(string.Join(',', data), Ascii);
#else
            Write(string.Join(",", data), Ascii);
#endif // NET || NETSTANDARD2_1_OR_GREATER
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

        /// <summary>
        /// Writes data into internal buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        protected void WriteBinary(byte[] buffer, int offset, int count)
        {
            _stream.WriteBinary(buffer, offset, count);
        }
    }
}
