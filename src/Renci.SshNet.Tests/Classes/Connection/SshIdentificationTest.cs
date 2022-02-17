using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Connection;
using System;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class SshIdentificationTest
    {
        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersion()
        {
            const string protocolVersion = "1.5";
            const string softwareVersion = "SSH.NET_2020.0.0";

            var sshIdentification = new SshIdentification(protocolVersion, softwareVersion);
            Assert.AreSame(protocolVersion, sshIdentification.ProtocolVersion);
            Assert.AreSame(softwareVersion, sshIdentification.SoftwareVersion);
            Assert.IsNull(sshIdentification.Comments);
        }

        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersion_ProtocolVersionIsNull()
        {
            const string protocolVersion = null;
            const string softwareVersion = "SSH.NET_2020.0.0";

            try
            {
                new SshIdentification(protocolVersion, softwareVersion);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("protocolVersion", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersion_SoftwareVersionIsNull()
        {
            const string protocolVersion = "2.0";
            const string softwareVersion = null;

            try
            {
                new SshIdentification(protocolVersion, softwareVersion);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("softwareVersion", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersionAndComments()
        {
            const string protocolVersion = "1.5";
            const string softwareVersion = "SSH.NET_2020.0.0";
            const string comments = "Beware, dangerous!";

            var sshIdentification = new SshIdentification(protocolVersion, softwareVersion, comments);
            Assert.AreSame(protocolVersion, sshIdentification.ProtocolVersion);
            Assert.AreSame(softwareVersion, sshIdentification.SoftwareVersion);
            Assert.AreSame(comments, sshIdentification.Comments);
        }

        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersionAndComments_CommentsIsNull()
        {
            const string protocolVersion = "1.5";
            const string softwareVersion = "SSH.NET_2020.0.0";
            const string comments = null;

            var sshIdentification = new SshIdentification(protocolVersion, softwareVersion, comments);
            Assert.AreSame(protocolVersion, sshIdentification.ProtocolVersion);
            Assert.AreSame(softwareVersion, sshIdentification.SoftwareVersion);
            Assert.IsNull(comments, sshIdentification.Comments);
        }

        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersionAndComments_ProtocolVersionIsNull()
        {
            const string protocolVersion = null;
            const string softwareVersion = "SSH.NET_2020.0.0";
            const string comments = "Beware!";

            try
            {
                new SshIdentification(protocolVersion, softwareVersion, comments);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("protocolVersion", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ctor_ProtocolVersionAndSoftwareVersionAndComments_SoftwareVersionIsNull()
        {
            const string protocolVersion = "2.0";
            const string softwareVersion = null;
            const string comments = "Beware!";

            try
            {
                new SshIdentification(protocolVersion, softwareVersion, comments);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("softwareVersion", ex.ParamName);
            }
        }

        [TestMethod]
        public void ToString_Comments()
        {
            var sshIdentification = new SshIdentification("2.0", "SSH.NET", "Beware, dangerous");
            Assert.AreEqual("SSH-2.0-SSH.NET Beware, dangerous", sshIdentification.ToString());
        }

        [TestMethod]
        public void ToString_CommentsIsNull()
        {
            var sshIdentification = new SshIdentification("2.0", "SSH.NET_2020.0.0");
            Assert.AreEqual("SSH-2.0-SSH.NET_2020.0.0", sshIdentification.ToString());

            sshIdentification = new SshIdentification("2.0", "SSH.NET_2020.0.0", null);
            Assert.AreEqual("SSH-2.0-SSH.NET_2020.0.0", sshIdentification.ToString());
        }
    }
}
