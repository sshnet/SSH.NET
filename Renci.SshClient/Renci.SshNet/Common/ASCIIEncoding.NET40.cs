/*
 * ASCIIEncoding.cs - Implementation of the "System.Text.ASCIIEncoding" class.
 *
 * Copyright (C) 2001  Southern Storm Software, Pty Ltd.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Text;

namespace Renci.SshNet.Common
{
    [Serializable]
    public partial class ASCIIEncoding : Encoding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ASCIIEncoding"/> class.
        /// </summary>
        public ASCIIEncoding()
            : base(ASCII_CODE_PAGE)
        {
        }

        /// <summary>
        /// Decodes all the bytes in the specified byte array into a string.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <returns>
        /// A <see cref="T:System.String"/> containing the results of decoding the specified sequence of bytes.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The byte array contains invalid Unicode code points.</exception>
        ///   
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="bytes"/> is null. </exception>
        ///   
        /// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Understanding Encodings for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback"/> is set to <see cref="T:System.Text.DecoderExceptionFallback"/>.</exception>
        public override String GetString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length == 0)
            {
                return String.Empty;
            }
            int count = bytes.Length;
            int posn = 0;

            char[] chars = new char[count];

            while (count-- > 0)
            {
                chars[posn] = (char)(bytes[posn]);
                ++posn;
            }
            return new string(chars);
        }    
    }; 
}
