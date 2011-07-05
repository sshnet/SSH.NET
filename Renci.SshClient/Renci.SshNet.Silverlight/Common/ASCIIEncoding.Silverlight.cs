using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace Renci.SshNet.Common
{
    public partial class ASCIIEncoding : Encoding
    {
        /// <summary>
        /// Gets the name registered with the
        /// Internet Assigned Numbers Authority (IANA) for the current encoding.
        /// </summary>
        /// <returns>
        /// The IANA name for the current <see cref="System.Text.Encoding"/>.
        /// </returns>
        public override string WebName
        {
            get
            {
                return "iso-8859-1";
            }
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
        public String GetString(byte[] bytes)
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
    }
}
