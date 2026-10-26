#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Renci.SshNet
{
    /// <summary>
    /// The <see cref="ActivitySource"/> and <see cref="Meter"/> used for SSH.NET's
    /// traces and metrics. Both are named <c>Renci.SshNet</c>.
    /// </summary>
    internal static class SshNetTelemetry
    {
        internal const string Name = "Renci.SshNet";

        internal static readonly ActivitySource ActivitySource = new(Name, ThisAssembly.NuGetPackageVersion);

        internal static readonly Meter Meter = new(Name, ThisAssembly.NuGetPackageVersion);

        private static readonly Histogram<double> OperationDuration = Meter.CreateHistogram<double>(
            "ssh.client.operation.duration",
            unit: "s",
            description: "Duration of SSH client operations.");

        /// <summary>
        /// Gets a value indicating whether anybody is listening to the traces or metrics.
        /// </summary>
        internal static bool IsEnabled
        {
            get { return OperationDuration.Enabled || ActivitySource.HasListeners(); }
        }

        /// <summary>
        /// Starts tracking an operation, or returns <see langword="null"/> if nobody is listening.
        /// </summary>
        internal static Operation? StartOperation(string protocol, string operation, IConnectionInfo? connectionInfo, string? path = null)
        {
            if (!IsEnabled)
            {
                return null;
            }

            return new Operation(protocol, operation, connectionInfo, path);
        }

        internal sealed class Operation : IDisposable
        {
            private readonly Activity? _activity;
            private readonly long _startTimestamp;
            private TagList _metricTags;
            private bool _isDisposed;

            internal Operation(string protocol, string operation, IConnectionInfo? connectionInfo, string? path)
            {
                _startTimestamp = Stopwatch.GetTimestamp();

                _metricTags = new TagList
                {
                    { "network.protocol.name", protocol },
                    { "ssh.operation.name", operation },
                };

                if (connectionInfo is not null)
                {
                    _metricTags.Add("server.address", connectionInfo.Host);
                    _metricTags.Add("server.port", connectionInfo.Port);
                }

                _activity = ActivitySource.StartActivity(protocol + " " + operation, ActivityKind.Client);

                if (_activity is { IsAllDataRequested: true })
                {
                    foreach (var tag in _metricTags)
                    {
                        _ = _activity.SetTag(tag.Key, tag.Value);
                    }

                    if (path is not null)
                    {
                        _ = _activity.SetTag("file.path", path);
                    }
                }
            }

            public void SetTag(string key, object? value)
            {
                if (_activity is { IsAllDataRequested: true })
                {
                    _ = _activity.SetTag(key, value);
                }
            }

            public void SetError(Exception exception)
            {
                SetError(exception.GetType().FullName!, exception.Message);
            }

            public void SetError(string errorType, string? description)
            {
                _metricTags.Add("error.type", errorType);

                if (_activity is not null)
                {
                    _ = _activity.SetTag("error.type", errorType);
                    _ = _activity.SetStatus(ActivityStatusCode.Error, description);
                }
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                var elapsedSeconds = (Stopwatch.GetTimestamp() - _startTimestamp) / (double)Stopwatch.Frequency;
                OperationDuration.Record(elapsedSeconds, _metricTags);

                _activity?.Dispose();
            }
        }
    }
}
