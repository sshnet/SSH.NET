Logging
=================

SSH.NET uses the [Microsoft.Extensions.Logging](https://learn.microsoft.com/dotnet/core/extensions/logging) API to log diagnostic messages. 

It is possible to specify a logger in the `ConnectionInfo`, for example:

```cs
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddConsole();
});

var connectionInfo = new ConnectionInfo("sftp.foo.com",
                                        "guest",
                                        new PasswordAuthenticationMethod("guest", "pwd"));
										
connectionInfo.LoggerFactory = loggerFactory;
using (var client = new SftpClient(connectionInfo))
{
    client.Connect();
}
```

You can also register an application-wide `ILoggerFactory` before using the SSH.NET APIs, this will be used as a fallback if the `ConnectionInfo` is not set, for example:

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
