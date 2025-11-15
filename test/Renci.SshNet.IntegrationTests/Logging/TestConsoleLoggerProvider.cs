#nullable enable

using Microsoft.Extensions.Logging;

namespace Renci.SshNet.IntegrationTests.Logging
{
    internal class TestConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new TestConsoleLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}
