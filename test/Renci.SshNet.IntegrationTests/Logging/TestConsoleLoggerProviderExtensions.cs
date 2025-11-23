#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Renci.SshNet.IntegrationTests.Logging
{
    internal static class TestConsoleLoggerProviderExtensions
    {
        internal static void AddTestConsoleLogger(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestConsoleLoggerProvider>());
        }
    }
}
