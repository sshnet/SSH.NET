namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    public sealed class LogLevelFormatter
    {
        public string Format(LogLevel logLevel)
        {
            return logLevel.ToString("G").ToUpperInvariant();
        }
    }
}
