using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Verifies that the length of a control line received from the server is bounded.
    /// </summary>
    [TestClass]
    public class ScpClientTest_Download_ControlLineTooLong
    {
        [TestMethod]
        public void LineLongerThanMaximum_ShouldThrowScpException()
        {
            var transcript = new ScpServerTranscript()
                .Raw("C0644 1 " + new string('a', ScpClient.MaxControlLineLength))
                .Raw("\n");

            using (var destination = new MemoryStream())
            {
                var result = transcript.Run(client => client.Download("/remote/file", destination), closeChannel: true);

                Assert.IsTrue(result.Completed);
                Assert.IsInstanceOfType<ScpException>(result.Exception);
                Assert.Contains("maximum length", result.Exception.Message, StringComparison.Ordinal);
            }
        }

        [TestMethod]
        public void LineOfMaximumLength_ShouldBeAccepted()
        {
            const string Header = "C0644 1 ";

            var transcript = new ScpServerTranscript()
                .Line(Header + new string('a', ScpClient.MaxControlLineLength - Header.Length))
                .Raw("x")
                .Raw([0]);

            using (var destination = new MemoryStream())
            {
                var result = transcript.Run(client => client.Download("/remote/file", destination), closeChannel: true);

                Assert.IsTrue(result.Completed);
                Assert.IsNull(result.Exception, result.Exception?.ToString());
                Assert.AreEqual(1, destination.Length);
            }
        }
    }
}
