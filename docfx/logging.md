Logging
=================

SSH.NET uses the `Microsoft.Extensions.Logging` API to log diagnostic messages. In order to access the log messages of SSH.NET in your own application for diagnosis, simply register your own `ILoggerFactory` before using the SSH.NET APIs with the following code:

```cs
SshNetLoggingConfiguration.InitializeLogging(loggerFactory);
```

All messages by SSH.NET are logged under the `Renci.SshNet` category.
