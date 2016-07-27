#if !FEATURE_ENCODING_ASCII

using System;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Implementation of ASCII Encoding
    /// </summary>
    public class ASCIIEncoding : Encoding
    {
        private readonly char _fallbackChar;

        private static readonly char[] ByteToChar;

        static ASCIIEncoding()
        {
            if (ByteToChar == null)
            {
                ByteToChar = new char[128];
                var ch = '\0';
                for (byte i = 0; i < 128; i++)
                {
                    ByteToChar[i] = ch++;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ASCIIEncoding"/> class.
        /// </summary>
        public ASCIIEncoding()
        {
            _fallbackChar = '?';
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding a set of characters from the specified character array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="index">The index of the first character to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>
        /// The number of bytes produced by encoding the specified characters.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="chars"/> is  <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> is less than zero.-or- <paramref name="index"/> and <paramref name="count"/> do not denote a valid range in <paramref name="chars"/>.</exception>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        /// <summary>
        /// Encodes a set of characters from the specified character array into the specified byte array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="charIndex">The index of the first character to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
        /// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
        /// <returns>
        /// The actual number of bytes written into <paramref name="bytes"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="chars"/> is  <c>null</c>.-or- <paramref name="bytes"/> is  <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="charIndex"/> or <paramref name="charCount"/> or <paramref name="byteIndex"/> is less than zero.-or- <paramref name="charIndex"/> and <paramref name="charCount"/> do not denote a valid range in <paramref name="chars"/>.-or- <paramref name="byteIndex"/> is not a valid index in <paramref name="bytes"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have enough capacity from <paramref name="byteIndex"/> to the end of the array to accommodate the resulting bytes.</exception>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (var i = 0; i < charCount && i < chars.Length; i++)
            {
                var b = (byte)chars[i + charIndex];

                if (b > 127)
                    b = (byte) _fallbackChar;

                bytes[i + byteIndex] = b;
            }
            return charCount;
        }

        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>
        /// The number of characters produced by decoding the specified sequence of bytes.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> is less than zero.-or- <paramref name="index"/> and <paramref name="count"/> do not denote a valid range in <paramref name="bytes"/>.</exception>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        /// <summary>
        /// Decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="byteIndex">The index of the first byte to decode.</param>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <param name="chars">The character array to contain the resulting set of characters.</param>
        /// <param name="charIndex">The index at which to start writing the resulting set of characters.</param>
        /// <returns>
        /// The actual number of characters written into <paramref name="chars"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is  <c>null</c>.-or- <paramref name="chars"/> is  <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteIndex"/> or <paramref name="byteCount"/> or <paramref name="charIndex"/> is less than zero.-or- <paramref name="byteIndex"/> and <paramref name="byteCount"/> do not denote a valid range in <paramref name="bytes"/>.-or- <paramref name="charIndex"/> is not a valid index in <paramref name="chars"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="chars"/> does not have enough capacity from <paramref name="charIndex"/> to the end of the array to accommodate the resulting characters.</exception>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (var i = 0; i < byteCount; i++)
            {
                var b = bytes[i + byteIndex];
                char ch;

                if (b > 127)
                {
                    ch = _fallbackChar;
                }
                else 
                {
                    ch = ByteToChar[b];
                }

                chars[i + charIndex] = ch;
            }
            return byteCount;
        }

        /// <summary>
        /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <returns>
        /// The maximum number of bytes produced by encoding the specified number of characters.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="charCount"/> is less than zero.</exception>
        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException("charCount", "Non-negative number required.");

            return charCount + 1;
        }

        /// <summary>
        /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <returns>
        /// The maximum number of characters produced by decoding the specified number of bytes.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is less than zero.</exception>
        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount", "Non-negative number required.");

            return byteCount;
        }
    }
}

#endif // !FEATURE_ENCODING_ASCII