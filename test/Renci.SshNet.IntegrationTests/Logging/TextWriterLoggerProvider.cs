#nullable enable

using Microsoft.Extensions.Logging;

namespace Renci.SshNet.IntegrationTests.Logging
{
    internal class TextWriterLoggerProvider(TextWriter writer) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new TextWriterLogger(writer, categoryName);
        }

        public void Dispose()
        {
        }
    }
}
