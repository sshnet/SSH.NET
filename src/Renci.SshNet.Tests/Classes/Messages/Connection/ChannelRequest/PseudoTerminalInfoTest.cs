using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    /// Represents "pty-req" type channel request information
    /// </summary>
    [TestClass]
    public class PseudoTerminalRequestInfoTest
    {
        private string _environmentVariable;
        private uint _columns;
        private uint _rows;
        private uint _width;
        private uint _height;
        private IDictionary<TerminalModes, uint> _terminalModeValues;
        private byte[] _environmentVariableBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _environmentVariable = random.Next().ToString(CultureInfo.InvariantCulture);
            _environmentVariableBytes = Encoding.UTF8.GetBytes(_environmentVariable);
            _columns = (uint) random.Next(0, int.MaxValue);
            _rows = (uint) random.Next(0, int.MaxValue);
            _width = (uint) random.Next(0, int.MaxValue);
            _height = (uint) random.Next(0, int.MaxValue);
            _terminalModeValues = new Dictionary<TerminalModes, uint>
            {
                {TerminalModes.CS8, 433},
                {TerminalModes.ECHO, 566}
            };
        }

        [TestMethod]
        public void GetBytes()
        {
            var target = new PseudoTerminalRequestInfo(_environmentVariable, _columns, _rows, _width, _height, _terminalModeValues);

            var bytes = target.GetBytes();

            var expectedBytesLength = 1; // WantReply
            expectedBytesLength += 4; // EnvironmentVariable length
            expectedBytesLength += _environmentVariableBytes.Length; // EnvironmentVariable
            expectedBytesLength += 4; // Columns
            expectedBytesLength += 4; // Rows
            expectedBytesLength += 4; // PixelWidth
            expectedBytesLength += 4; // PixelHeight
            expectedBytesLength += 4; // Length of "encoded terminal modes"
            expectedBytesLength += _terminalModeValues.Count*(1 + 4) + 1; // encoded terminal modes

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual(1, sshDataStream.ReadByte()); // WantReply
            Assert.AreEqual(_environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
            Assert.AreEqual(_columns, sshDataStream.ReadUInt32());
            Assert.AreEqual(_rows, sshDataStream.ReadUInt32());
            Assert.AreEqual(_width, sshDataStream.ReadUInt32());
            Assert.AreEqual(_height, sshDataStream.ReadUInt32());
            Assert.AreEqual((uint) (_terminalModeValues.Count * (1 + 4) + 1), sshDataStream.ReadUInt32());
            Assert.AreEqual((int) TerminalModes.CS8, sshDataStream.ReadByte());
            Assert.AreEqual(_terminalModeValues[TerminalModes.CS8], sshDataStream.ReadUInt32());
            Assert.AreEqual((int) TerminalModes.ECHO, sshDataStream.ReadByte());
            Assert.AreEqual(_terminalModeValues[TerminalModes.ECHO], sshDataStream.ReadUInt32());
            Assert.AreEqual((int) TerminalModes.TTY_OP_END, sshDataStream.ReadByte());

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void GetBytes_TerminalModeValues_Null()
        {
            var target = new PseudoTerminalRequestInfo(_environmentVariable, _columns, _rows, _width, _height, null);

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
            Assert.AreEqual(_environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
            Assert.AreEqual(_columns, sshDataStream.ReadUInt32());
            Assert.AreEqual(_rows, sshDataStream.ReadUInt32());
            Assert.AreEqual(_width, sshDataStream.ReadUInt32());
            Assert.AreEqual(_height, sshDataStream.ReadUInt32());
            Assert.AreEqual((uint) 0, sshDataStream.ReadUInt32());

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void GetBytes_TerminalModeValues_Empty()
        {
            var target = new PseudoTerminalRequestInfo(_environmentVariable,
                                                       _columns,
                                                       _rows,
                                                       _width,
                                                       _height,
                                                       new Dictionary<TerminalModes, uint>());

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
            Assert.AreEqual(_environmentVariable, sshDataStream.ReadString(Encoding.UTF8));
            Assert.AreEqual(_columns, sshDataStream.ReadUInt32());
            Assert.AreEqual(_rows, sshDataStream.ReadUInt32());
            Assert.AreEqual(_width, sshDataStream.ReadUInt32());
            Assert.AreEqual(_height, sshDataStream.ReadUInt32());
            Assert.AreEqual((uint) 0, sshDataStream.ReadUInt32());

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void DefaultCtor()
        {
            var ptyReq = new PseudoTerminalRequestInfo();

            Assert.IsTrue(ptyReq.WantReply);
            Assert.AreEqual(uint.MinValue, ptyReq.Columns);
            Assert.IsNull(ptyReq.EnvironmentVariable);
            Assert.AreEqual("pty-req", ptyReq.RequestName);
            Assert.AreEqual(uint.MinValue, ptyReq.PixelHeight);
            Assert.AreEqual(uint.MinValue, ptyReq.PixelWidth);
            Assert.AreEqual(uint.MinValue, ptyReq.Rows);
            Assert.IsNull(ptyReq.TerminalModeValues);
        }

        [TestMethod]
        public void FullCtor()
        {
            var ptyReq = new PseudoTerminalRequestInfo(_environmentVariable, _columns, _rows, _width, _height, _terminalModeValues);

            Assert.IsTrue(ptyReq.WantReply);
            Assert.AreEqual(_columns, ptyReq.Columns);
            Assert.AreSame(_environmentVariable, ptyReq.EnvironmentVariable);
            Assert.AreEqual("pty-req", ptyReq.RequestName);
            Assert.AreEqual(_height, ptyReq.PixelHeight);
            Assert.AreEqual(_width, ptyReq.PixelWidth);
            Assert.AreEqual(_rows, ptyReq.Rows);
            Assert.AreSame(_terminalModeValues, ptyReq.TerminalModeValues);
        }

        [TestMethod]
        public void NameShouldReturnPtyReq()
        {
            Assert.AreEqual("pty-req", PseudoTerminalRequestInfo.Name);
        }
    }
}