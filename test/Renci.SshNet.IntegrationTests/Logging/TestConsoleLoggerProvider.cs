#nullable enable

using Microsoft.Extensions.Logging;

namespace Renci.SshNet.IntegrationTests.Logging
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class TestConsoleLoggerProvider : ILoggerProvider
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
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
