namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal sealed class LogLevelFormatter
    {
        public string Format(LogLevel logLevel)
        {
            return logLevel.ToString("G").ToUpperInvariant();
        }
    }
}
