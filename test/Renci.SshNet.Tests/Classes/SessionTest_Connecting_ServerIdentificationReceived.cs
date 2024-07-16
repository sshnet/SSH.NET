﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerIdentificationReceived : SessionTest_ConnectingBase
    {
        protected override void SetupData()
        {
            base.SetupData();

            Session.ServerIdentificationReceived += (s, e) =>
            {
                if ((e.SshIdentification.SoftwareVersion.StartsWith("OpenSSH_6.5", System.StringComparison.Ordinal) || e.SshIdentification.SoftwareVersion.StartsWith("OpenSSH_6.6", System.StringComparison.Ordinal))
                       && !e.SshIdentification.SoftwareVersion.StartsWith("OpenSSH_6.6.1", System.StringComparison.Ordinal))
                {
                    _ = ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256");
                    _ = ConnectionInfo.KeyExchangeAlgorithms.Remove("curve25519-sha256@libssh.org");
                }
            };
        }

        [TestMethod]
        [DataRow("OpenSSH_6.5")]
        [DataRow("OpenSSH_6.5p1")]
        [DataRow("OpenSSH_6.5 PKIX")]
        [DataRow("OpenSSH_6.6")]
        [DataRow("OpenSSH_6.6p1")]
        [DataRow("OpenSSH_6.6 PKIX")]
        public void ShouldExcludeCurve25519KexWhenServerIs(string softwareVersion)
        {
            ServerIdentification = new SshIdentification("2.0", softwareVersion);

            Session.Connect();

            Assert.IsFalse(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256"));
            Assert.IsFalse(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256@libssh.org"));
        }

        [TestMethod]
        [DataRow("OpenSSH_6.6.1")]
        [DataRow("OpenSSH_6.6.1p1")]
        [DataRow("OpenSSH_6.6.1 PKIX")]
        [DataRow("OpenSSH_6.7")]
        [DataRow("OpenSSH_6.7p1")]
        [DataRow("OpenSSH_6.7 PKIX")]
        public void ShouldIncludeCurve25519KexWhenServerIs(string softwareVersion)
        {
            ServerIdentification = new SshIdentification("2.0", softwareVersion);

            Session.Connect();

            Assert.IsTrue(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256"));
            Assert.IsTrue(ConnectionInfo.KeyExchangeAlgorithms.ContainsKey("curve25519-sha256@libssh.org"));
        }
    }
}
