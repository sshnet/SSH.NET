Logging
=================

SSH.NET uses the [Microsoft.Extensions.Logging](https://learn.microsoft.com/dotnet/core/extensions/logging) API to log diagnostic messages. In order to access the log messages of SSH.NET in your own application for diagnosis, register your own `ILoggerFactory` before using the SSH.NET APIs, for example:

```cs
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddConsole();
});

Renci.SshNet.SshNetLoggingConfiguration.InitializeLogging(loggerFactory);
```

All messages by SSH.NET are logged under the `Renci.SshNet` category.
