using System.Globalization;

namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    public sealed class Int32Formatter
    {
        public string Format(int value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
