using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Renci.SshNet.Common
{
    internal static class PacketDump
    {
        public static string Create(List<byte> data, int indentLevel)
        {
            return Create(data.ToArray(), indentLevel);
        }

        public static string Create(byte[] data, int indentLevel)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (indentLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indentLevel), "Cannot be less than zero.");
            }

            const int lineWidth = 16;

            var result = new StringBuilder();
            var line = new byte[lineWidth];
            var indentChars = new string(' ', indentLevel);

            for (var pos = 0; pos < data.Length;)
            {
                var linePos = 0;

                if (result.Length > 0)
                {
                    _ = result.Append(Environment.NewLine);
                }

                _ = result.Append(indentChars);
                _ = result.Append(pos.ToString("X8"));
                _ = result.Append("  ");

                while (true)
                {
                    line[linePos++] = data[pos++];

                    if (linePos == lineWidth || pos == data.Length)
                    {
                        break;
                    }
                }

                _ = result.Append(AsHex(line, linePos));
                _ = result.Append("  ");
                _ = result.Append(AsAscii(line, linePos));
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
                    _ = hex.Append(' ');
                }

                _ = hex.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            if (length < data.Length)
            {
                _ = hex.Append(new string(' ', (data.Length - length) * 3));
            }

            return hex.ToString();
        }

        private static string AsAscii(byte[] data, int length)
        {
            var encoding = Encoding.ASCII;

            var ascii = new StringBuilder();
            const char dot = '.';

            for (var i = 0; i < length; i++)
            {
                var b = data[i];

                if (b is < 32 or >= 127)
                {
                    _ = ascii.Append(dot);
                }
                else
                {
                    _ = ascii.Append(encoding.GetString(data, i, 1));
                }
            }

            return ascii.ToString();
        }
    }
}
