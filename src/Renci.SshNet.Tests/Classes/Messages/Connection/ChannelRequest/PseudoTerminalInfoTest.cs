using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    /// Represents "pty-req" type channel request information
    /// </summary>
    [TestClass]
    public class PseudoTerminalRequestInfoTest : TestBase
    {
        private string environmentVariable;
        private uint columns;
        private uint rows;
        private uint width;
        private uint height;
        IDictionary<TerminalModes, uint> terminalModeValues;
        private byte[] _environmentVariableBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            environmentVariable = random.Next().ToString(CultureInfo.InvariantCulture);
            columns = (uint) random.Next(0, int.MaxValue);
            rows = (uint) random.Next(0, int.MaxValue);
            width = (uint) random.Next(0, int.MaxValue);
            height = (uint) random.Next(0, int.MaxValue);
            terminalModeValues = new Dictionary<TerminalModes, uint>();


            _environmentVariableBytes = Encoding.UTF8.GetBytes(environmentVariable);
        }

        [TestMethod]
        public void GetBytes_TerminalModeValues_Null()
        {
            var target = new PseudoTerminalRequestInfo(environmentVariable, columns, rows, width, height, null);

            var bytes = target.GetBytes();

            var expectedBytesLength = 1; // WantReply
            expectedBytesLength += 4; // EnvironmentVariable length
            expectedBytesLength += _environmentVariableBytes.Length; // EnvironmentVariable
            expectedBytesLength += 4; // Columns
            expectedBytesLength += 4; // Rows
            expectedBytesLength += 4; // PixelWidth
            expectedBytesLength += 4; // PixelHeight
            expectedBytesLength += 4; // Length of "encoded terminal modes"

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual(1, sshDataStream.ReadByte()); // WantReply
//            Assert.AreEqual((uint) _environmentVariableBytes.Length, sshDataStream.ReadUInt32());
            Assert.AreEqual(environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
            Assert.AreEqual(columns, sshDataStream.ReadUInt32());
            Assert.AreEqual(rows, sshDataStream.ReadUInt32());
            Assert.AreEqual(width, sshDataStream.ReadUInt32());
            Assert.AreEqual(height, sshDataStream.ReadUInt32());
            Assert.AreEqual(0, sshDataStream.ReadUInt32());

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}