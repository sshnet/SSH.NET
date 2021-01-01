using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Renci.SshNet.Common
{
    internal class PacketDump
    {
        public static string Create(List<byte> data, int indentLevel)
        {
            return Create(data.ToArray(), indentLevel);
        }

        public static string Create(byte[] data, int indentLevel)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (indentLevel < 0)
                throw new ArgumentOutOfRangeException("indentLevel", "Cannot be less than zero.");

            const int lineWidth = 16;

            var result = new StringBuilder();
            var line = new byte[lineWidth];
            var indentChars = new string(' ', indentLevel);

            for (var pos = 0; pos < data.Length; )
            {
                var linePos = 0;

                if (result.Length > 0)
                {
                    result.Append(Environment.NewLine);
                }

                result.Append(indentChars);
                result.Append(pos.ToString("X8"));
                result.Append("  ");

                while (true)
                {
                    line[linePos++] = data[pos++];

                    if (linePos == lineWidth || pos == data.Length)
                    {
                        break;
                    }
                }

                result.Append(AsHex(line, linePos));
                result.Append("  ");
                result.Append(AsAscii(line, linePos));
            }
            return result.ToString();
        }

        private static string AsHex(byte[] data, int length)
        {
            var hex = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                if (i > 0)
                {
                    hex.Append(' ');
                }

                hex.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            if (length < data.Length)
            {
                hex.Append(new string(' ', (data.Length - length) * 3));
            }

            return hex.ToString();
        }

        private static string AsAscii(byte[] data, int length)
        {
#if FEATURE_ENCODING_ASCII
        var encoding = Encoding.ASCII;
#else
        var encoding = new ASCIIEncoding();
#endif

        var ascii = new StringBuilder();
            const char dot = '.';

            for (var i = 0; i < length; i++)
            {
                var b = data[i];

                if (b < 32 || b >= 127)
                {
                    ascii.Append(dot);
                }
                else
                {
                    ascii.Append(encoding.GetString(data, i, 1));
                }
            }

            return ascii.ToString();
        }
    }
}