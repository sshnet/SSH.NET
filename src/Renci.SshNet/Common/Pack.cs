namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides convenience methods for conversion to and from both Big Endian and Little Endian.
    /// </summary>
    internal static class Pack
    {
        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ushort" />.</returns>
        internal static ushort LittleEndianToUInt16(byte[] buffer)
        {
            ushort n = buffer[0];
            n |= (ushort) (buffer[1] << 8);
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint LittleEndianToUInt32(byte[] buffer, int offset)
        {
            uint n = buffer[offset];
            n |= (uint) buffer[offset + 1] << 8;
            n |= (uint) buffer[offset + 2] << 16;
            n |= (uint) buffer[offset + 3] << 24;
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint LittleEndianToUInt32(byte[] buffer)
        {
            uint n = buffer[0];
            n |= (uint) buffer[1] << 8;
            n |= (uint) buffer[2] << 16;
            n |= (uint) buffer[3] << 24;
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ulong" />.</returns>
        internal static ulong LittleEndianToUInt64(byte[] buffer)
        {
            ulong n = buffer[0];
            n |= (ulong) buffer[1] << 8;
            n |= (ulong) buffer[2] << 16;
            n |= (ulong) buffer[3] << 24;
            n |= (ulong) buffer[4] << 32;
            n |= (ulong) buffer[5] << 40;
            n |= (ulong) buffer[6] << 48;
            n |= (ulong) buffer[7] << 56;
            return n;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        internal static byte[] UInt16ToLittleEndian(ushort value)
        {
            var buffer = new byte[2];
            UInt16ToLittleEndian(value, buffer);
            return buffer;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        internal static void UInt16ToLittleEndian(ushort value, byte[] buffer)
        {
            buffer[0] = (byte) (value & 0x00FF);
            buffer[1] = (byte) ((value & 0xFF00) >> 8);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        internal static byte[] UInt32ToLittleEndian(uint value)
        {
            var buffer = new byte[4];
            UInt32ToLittleEndian(value, buffer);
            return buffer;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        internal static void UInt32ToLittleEndian(uint value, byte[] buffer)
        {
            buffer[0] = (byte) (value & 0x000000FF);
            buffer[1] = (byte) ((value & 0x0000FF00) >> 8);
            buffer[2] = (byte) ((value & 0x00FF0000) >> 16);
            buffer[3] = (byte) ((value & 0xFF000000) >> 24);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        internal static void UInt32ToLittleEndian(uint value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte) (value & 0x000000FF);
            buffer[offset + 1] = (byte) ((value & 0x0000FF00) >> 8);
            buffer[offset + 2] = (byte) ((value & 0x00FF0000) >> 16);
            buffer[offset + 3] = (byte) ((value & 0xFF000000) >> 24);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        internal static byte[] UInt64ToLittleEndian(ulong value)
        {
            var buffer = new byte[8];
            UInt64ToLittleEndian(value, buffer);
            return buffer;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        internal static void UInt64ToLittleEndian(ulong value, byte[] buffer)
        {
            buffer[0] = (byte) (value & 0x00000000000000FF);
            buffer[1] = (byte) ((value & 0x000000000000FF00) >> 8);
            buffer[2] = (byte) ((value & 0x0000000000FF0000) >> 16);
            buffer[3] = (byte) ((value & 0x00000000FF000000) >> 24);
            buffer[4] = (byte) ((value & 0x000000FF00000000) >> 32);
            buffer[5] = (byte) ((value & 0x0000FF0000000000) >> 40);
            buffer[6] = (byte) ((value & 0x00FF000000000000) >> 48);
            buffer[7] = (byte) ((value & 0xFF00000000000000) >> 56);
        }

        internal static byte[] UInt16ToBigEndian(ushort value)
        {
            var buffer = new byte[2];
            UInt16ToBigEndian(value, buffer);
            return buffer;
        }

        internal static void UInt16ToBigEndian(ushort value, byte[] buffer)
        {
            buffer[0] = (byte) (value >> 8);
            buffer[1] = (byte) (value & 0x00FF);
        }

        internal static void UInt16ToBigEndian(ushort value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte) (value >> 8);
            buffer[offset + 1] = (byte) (value & 0x00FF);
        }

        internal static void UInt32ToBigEndian(uint value, byte[] buffer)
        {
            buffer[0] = (byte) ((value & 0xFF000000) >> 24);
            buffer[1] = (byte) ((value & 0x00FF0000) >> 16);
            buffer[2] = (byte) ((value & 0x0000FF00) >> 8);
            buffer[3] = (byte) (value & 0x000000FF);
        }

        internal static void UInt32ToBigEndian(uint value, byte[] buffer, int offset)
        {
            buffer[offset++] = (byte) ((value & 0xFF000000) >> 24);
            buffer[offset++] = (byte) ((value & 0x00FF0000) >> 16);
            buffer[offset++] = (byte) ((value & 0x0000FF00) >> 8);
            buffer[offset] = (byte) (value & 0x000000FF);
        }

        internal static byte[] UInt32ToBigEndian(uint value)
        {
            var buffer = new byte[4];
            UInt32ToBigEndian(value, buffer);
            return buffer;
        }

        /// <summary>
        /// Returns the specified 64-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        internal static byte[] UInt64ToBigEndian(ulong value)
        {
            return new[]
                {
                    (byte) ((value & 0xFF00000000000000) >> 56),
                    (byte) ((value & 0x00FF000000000000) >> 48),
                    (byte) ((value & 0x0000FF0000000000) >> 40),
                    (byte) ((value & 0x000000FF00000000) >> 32),
                    (byte) ((value & 0x00000000FF000000) >> 24),
                    (byte) ((value & 0x0000000000FF0000) >> 16),
                    (byte) ((value & 0x000000000000FF00) >> 8),
                    (byte) (value & 0x00000000000000FF)
                };
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ushort" />.</returns>
        internal static ushort BigEndianToUInt16(byte[] buffer)
        {
            return (ushort) (buffer[0] << 8 | buffer[1]);
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint BigEndianToUInt32(byte[] buffer, int offset)
        {
            return (uint) buffer[offset + 0] << 24 |
                   (uint) buffer[offset + 1] << 16 |
                   (uint) buffer[offset + 2] << 8 |
                   buffer[offset + 3];
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint BigEndianToUInt32(byte[] buffer)
        {
            return (uint) buffer[0] << 24 |
                   (uint) buffer[1] << 16 |
                   (uint) buffer[2] << 8 |
                   buffer[3];
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ulong" />.</returns>
        internal static ulong BigEndianToUInt64(byte[] buffer)
        {
            return (ulong) buffer[0] << 56 |
                   (ulong) buffer[1] << 48 |
                   (ulong) buffer[2] << 40 |
                   (ulong) buffer[3] << 32 |
                   (ulong) buffer[4] << 24 |
                   (ulong) buffer[5] << 16 |
                   (ulong) buffer[6] << 8 |
                   buffer[7];
        }
    }
}
