#nullable enable
using Microsoft.Extensions.Logging;

namespace Renci.SshNet.IntegrationTests.Logging
{
    internal class TestConsoleLogger(string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(logLevel);
            sb.Append(": ");
            sb.Append(categoryName);
            sb.Append(": ");

            string message = formatter(state, exception);
            sb.Append(message);

            if (exception != null)
            {
                sb.Append(": ");
                sb.Append(exception);
            }

            string line = sb.ToString();
            Console.WriteLine(line);
        }
    }
}
