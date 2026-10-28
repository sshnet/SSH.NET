using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class TelemetryTests : TestBase, IDisposable
    {
        private readonly ActivitySource _testSource = new ActivitySource("Renci.SshNet.IntegrationTests");
        private readonly ConcurrentQueue<Activity> _activities = new ConcurrentQueue<Activity>();
        private readonly ConcurrentQueue<(double Value, Dictionary<string, object> Tags)> _durations = new ConcurrentQueue<(double, Dictionary<string, object>)>();
        private readonly ActivityListener _activityListener;
        private readonly MeterListener _meterListener;

        public TelemetryTests()
        {
            // .NET Framework defaults to hierarchical IDs; OpenTelemetry forces W3C, so do the same.
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            _activityListener = new ActivityListener
            {
                ShouldListenTo = source => string.Equals(source.Name, "Renci.SshNet", StringComparison.Ordinal) || string.Equals(source.Name, _testSource.Name, StringComparison.Ordinal),
                Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity =>
                {
                    if (activity.Source.Name == "Renci.SshNet")
                    {
                        _activities.Enqueue(activity);
                    }
                },
            };
            ActivitySource.AddActivityListener(_activityListener);

            _meterListener = new MeterListener
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Meter.Name == "Renci.SshNet")
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                },
            };
            _meterListener.SetMeasurementEventCallback<double>((instrument, value, tags, state) =>
            {
                var tagDictionary = new Dictionary<string, object>();
                foreach (var tag in tags)
                {
                    tagDictionary[tag.Key] = tag.Value;
                }

                _durations.Enqueue((value, tagDictionary));
            });
            _meterListener.Start();
        }

        public void Dispose()
        {
            _activityListener.Dispose();
            _meterListener.Dispose();
            _testSource.Dispose();
        }

        [TestMethod]
        public async Task Sftp_Operations_ProduceSpansAndDurations()
        {
            var directory = $"/tmp/telemetry-{Guid.NewGuid():N}";
            var path = $"{directory}/file.bin";

            using (var parent = _testSource.StartActivity("parent"))
            using (var client = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await client.ConnectAsync(CancellationToken.None);
                client.CreateDirectory(directory);

                using (var input = CreateMemoryStream(10_000))
                {
                    input.Position = 0;
                    await client.UploadFileAsync(input, path);
                }

                _ = client.ListDirectory(directory);

                await foreach (var file in client.ListDirectoryAsync(directory, CancellationToken.None))
                {
                    Assert.IsNotNull(file);
                }

                using (var output = new MemoryStream())
                {
                    client.DownloadFile(path, output);
                }

                client.DeleteFile(path);
                client.DeleteDirectory(directory);

                var spans = _activities.ToList();

                CollectionAssert.AreEqual(
                    new[]
                    {
                        "sftp Connect",
                        "sftp CreateDirectory",
                        "sftp UploadFile",
                        "sftp ListDirectory",
                        "sftp ListDirectory",
                        "sftp DownloadFile",
                        "sftp DeleteFile",
                        "sftp DeleteDirectory",
                    },
                    spans.Select(s => s.DisplayName).ToArray());

                foreach (var span in spans)
                {
                    Assert.AreEqual(ActivityKind.Client, span.Kind);
                    Assert.AreEqual(parent.TraceId, span.TraceId);
                    Assert.AreEqual(parent.SpanId, span.ParentSpanId);
                    Assert.AreEqual(ActivityStatusCode.Unset, span.Status);
                    Assert.AreEqual("sftp", span.GetTagItem("network.protocol.name"));
                    Assert.AreEqual(SshServerHostName, span.GetTagItem("server.address"));
                    Assert.AreEqual((int)SshServerPort, span.GetTagItem("server.port"));
                }

                var upload = spans.Single(s => s.DisplayName == "sftp UploadFile");
                Assert.AreEqual(path, upload.GetTagItem("file.path"));
                Assert.AreEqual(10_000L, upload.GetTagItem("sftp.transfer.bytes"));

                var download = spans.Single(s => s.DisplayName == "sftp DownloadFile");
                Assert.AreEqual(10_000L, download.GetTagItem("sftp.transfer.bytes"));

                Assert.AreEqual(spans.Count, _durations.Count);
                Assert.IsTrue(_durations.All(d => d.Value > 0 && !d.Tags.ContainsKey("file.path")));
            }
        }

        [TestMethod]
        public async Task Sftp_MetadataOperations_ProduceSpans()
        {
            var directory = $"/tmp/telemetry-{Guid.NewGuid():N}";

            using var client = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();

            await client.CreateDirectoryAsync(directory);
            client.WriteAllText($"{directory}/a.txt", "a");

            Assert.IsTrue(client.Exists($"{directory}/a.txt"));
            Assert.IsFalse(await client.ExistsAsync($"{directory}/missing.txt"));
            client.RenameFile($"{directory}/a.txt", $"{directory}/b.txt");
            _ = await client.GetAttributesAsync($"{directory}/b.txt", CancellationToken.None);
            client.ChangeDirectory(directory);
            await client.ChangeDirectoryAsync("/tmp");
            client.Delete($"{directory}/b.txt");
            await client.DeleteDirectoryAsync(directory);

            var spans = _activities.ToList();

            CollectionAssert.AreEqual(
                new[]
                {
                    "sftp Connect",
                    "sftp CreateDirectory",
                    "sftp Exists",
                    "sftp Exists",
                    "sftp RenameFile",
                    "sftp GetAttributes",
                    "sftp ChangeDirectory",
                    "sftp ChangeDirectory",
                    "sftp Get",
                    "sftp Delete",
                    "sftp DeleteDirectory",
                },
                spans.Select(s => s.DisplayName).ToArray());

            // A path that does not exist is a normal result of Exists, not an error.
            Assert.IsTrue(spans.TrueForAll(s => s.Status == ActivityStatusCode.Unset));

            var rename = spans.Single(s => s.DisplayName == "sftp RenameFile");
            Assert.AreEqual($"{directory}/a.txt", rename.GetTagItem("file.path"));

            // Delete looks the file up first, so Get is its child.
            var delete = spans.Single(s => s.DisplayName == "sftp Delete");
            var get = spans.Single(s => s.DisplayName == "sftp Get");
            Assert.AreEqual(delete.SpanId, get.ParentSpanId);
        }

        [TestMethod]
        public void Sftp_DownloadMissingFile_RecordsError()
        {
            using var client = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();

            using var output = new MemoryStream();
            _ = Assert.ThrowsExactly<SftpPathNotFoundException>(() => client.DownloadFile($"/tmp/missing-{Guid.NewGuid():N}", output));

            var span = _activities.Single(s => s.DisplayName == "sftp DownloadFile");
            Assert.AreEqual(ActivityStatusCode.Error, span.Status);
            Assert.AreEqual(typeof(SftpPathNotFoundException).FullName, span.GetTagItem("error.type"));

            var duration = _durations.Single(d => (string)d.Tags["ssh.operation.name"] == "DownloadFile");
            Assert.AreEqual(typeof(SftpPathNotFoundException).FullName, duration.Tags["error.type"]);
        }

        [TestMethod]
        public void Connect_WrongPassword_RecordsError()
        {
            using var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, "wrong-password");

            _ = Assert.ThrowsExactly<SshAuthenticationException>(client.Connect);

            var span = _activities.Single();
            Assert.AreEqual("ssh Connect", span.DisplayName);
            Assert.AreEqual(ActivityStatusCode.Error, span.Status);
            Assert.AreEqual(typeof(SshAuthenticationException).FullName, span.GetTagItem("error.type"));
        }

        [TestMethod]
        public async Task SshCommand_ExecuteAsync_EndsSpanBeforeAwaitResumes()
        {
            using var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();

            using var parent = _testSource.StartActivity("parent");
            using var command = client.CreateCommand("echo hello; exit 3");

            var task = command.ExecuteAsync();

            // ExecuteAsync is not async, so it must not leave its span as Activity.Current.
            Assert.AreSame(parent, Activity.Current);

            await task;

            var span = _activities.Single(s => s.DisplayName == "ssh Execute");
            Assert.AreEqual(parent.SpanId, span.ParentSpanId);
            Assert.AreEqual(3, span.GetTagItem("ssh.command.exit_code"));
            Assert.AreEqual(ActivityStatusCode.Unset, span.Status);
            Assert.IsNull(span.GetTagItem("ssh.command.text"));
        }

        [TestMethod]
        public void SshCommand_Timeout_RecordsError()
        {
            using var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();

            using var command = client.CreateCommand("sleep 10");
            command.CommandTimeout = TimeSpan.FromMilliseconds(200);

            _ = Assert.ThrowsExactly<SshOperationTimeoutException>(command.Execute);

            var span = _activities.Single(s => s.DisplayName == "ssh Execute");
            Assert.AreEqual(ActivityStatusCode.Error, span.Status);
            Assert.AreEqual(typeof(SshOperationTimeoutException).FullName, span.GetTagItem("error.type"));
        }
    }
}
