using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Represents abstract class for all Block Cipher implementations.
    /// </summary>
    public abstract class CipherBase
    {
        /// <summary>
        /// Gets the cipher key.
        /// </summary>
        public byte[] Key { get; private set; }

        /// <summary>
        /// Gets the initialization vector.
        /// </summary>
        public byte[] IV { get; private set; }

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public abstract int BlockSize { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherBase"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        public CipherBase(byte[] key, byte[] iv)
        {
            this.Key = key;
            this.IV = iv;
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to encrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write encrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        public abstract int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to decrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write decrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        public abstract int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        #region Packing functions

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt32ToBigEndian(uint number, byte[] buffer)
        {
            buffer[0] = (byte)(number >> 24);
            buffer[1] = (byte)(number >> 16);
            buffer[2] = (byte)(number >> 8);
            buffer[3] = (byte)(number);
        }

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt32ToBigEndian(uint number, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(number >> 24);
            buffer[++offset] = (byte)(number >> 16);
            buffer[++offset] = (byte)(number >> 8);
            buffer[++offset] = (byte)(number);
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        protected static uint BigEndianToUInt32(byte[] buffer)
        {
            uint n = (uint)buffer[0] << 24;
            n |= (uint)buffer[1] << 16;
            n |= (uint)buffer[2] << 8;
            n |= (uint)buffer[3];
            return n;
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns></returns>
        protected static uint BigEndianToUInt32(byte[] buffer, int offset)
        {
            uint n = (uint)buffer[offset] << 24;
            n |= (uint)buffer[++offset] << 16;
            n |= (uint)buffer[++offset] << 8;
            n |= (uint)buffer[++offset];
            return n;
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        protected static ulong BigEndianToUInt64(byte[] buffer)
        {
            uint hi = BigEndianToUInt32(buffer);
            uint lo = BigEndianToUInt32(buffer, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns></returns>
        protected static ulong BigEndianToUInt64(byte[] buffer, int offset)
        {
            uint hi = BigEndianToUInt32(buffer, offset);
            uint lo = BigEndianToUInt32(buffer, offset + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt64ToBigEndian(ulong number, byte[] buffer)
        {
            UInt32ToBigEndian((uint)(number >> 32), buffer);
            UInt32ToBigEndian((uint)(number), buffer, 4);
        }

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt64ToBigEndian(ulong number, byte[] buffer, int offset)
        {
            UInt32ToBigEndian((uint)(number >> 32), buffer, offset);
            UInt32ToBigEndian((uint)(number), buffer, offset + 4);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt32ToLittleEndian(uint number, byte[] buffer)
        {
            buffer[0] = (byte)(number);
            buffer[1] = (byte)(number >> 8);
            buffer[2] = (byte)(number >> 16);
            buffer[3] = (byte)(number >> 24);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt32ToLittleEndian(uint number, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(number);
            buffer[++offset] = (byte)(number >> 8);
            buffer[++offset] = (byte)(number >> 16);
            buffer[++offset] = (byte)(number >> 24);
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        protected static uint LittleEndianToUInt32(byte[] buffer)
        {
            uint n = (uint)buffer[0];
            n |= (uint)buffer[1] << 8;
            n |= (uint)buffer[2] << 16;
            n |= (uint)buffer[3] << 24;
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns></returns>
        protected static uint LittleEndianToUInt32(byte[] buffer, int offset)
        {
            uint n = (uint)buffer[offset];
            n |= (uint)buffer[++offset] << 8;
            n |= (uint)buffer[++offset] << 16;
            n |= (uint)buffer[++offset] << 24;
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        protected static ulong LittleEndianToUInt64(byte[] buffer)
        {
            uint lo = LittleEndianToUInt32(buffer);
            uint hi = LittleEndianToUInt32(buffer, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns></returns>
        protected static ulong LittleEndianToUInt64(byte[] buffer, int offset)
        {
            uint lo = LittleEndianToUInt32(buffer, offset);
            uint hi = LittleEndianToUInt32(buffer, offset + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt64ToLittleEndian(ulong number, byte[] buffer)
        {
            UInt32ToLittleEndian((uint)(number), buffer);
            UInt32ToLittleEndian((uint)(number >> 32), buffer, 4);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt64ToLittleEndian(ulong number, byte[] buffer, int offset)
        {
            UInt32ToLittleEndian((uint)(number), buffer, offset);
            UInt32ToLittleEndian((uint)(number >> 32), buffer, offset + 4);
        }

        #endregion
    }
}
