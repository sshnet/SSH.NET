using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ShellStreamTest_ReadExpect
    {
        private ShellStream _shellStream;
        private ChannelSessionStub _channelSessionStub;

        [TestInitialize]
        public void Initialize()
        {
            _channelSessionStub = new ChannelSessionStub();

            var connectionInfoMock = new Mock<IConnectionInfo>();

            connectionInfoMock.Setup(p => p.Encoding).Returns(Encoding.UTF8);

            var sessionMock = new Mock<ISession>();

            sessionMock.Setup(p => p.ConnectionInfo).Returns(connectionInfoMock.Object);
            sessionMock.Setup(p => p.CreateChannelSession()).Returns(_channelSessionStub);

            _shellStream = new ShellStream(
                sessionMock.Object,
                "terminalName",
                columns: 80,
                rows: 24,
                width: 800,
                height: 600,
                terminalModeValues: null,
                bufferSize: 1024,
                expectSize: 1024);
        }

        [TestMethod]
        public void Read_String()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            Assert.AreEqual("Hello World!", _shellStream.Read());
        }

        [TestMethod]
        public void Read_Bytes()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            byte[] buffer = new byte[12];

            Assert.AreEqual(7, _shellStream.Read(buffer, 3, 7));
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("\0\0\0Hello W\0\0"), buffer);

            Assert.AreEqual(5, _shellStream.Read(buffer, 0, 12));
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("orld!llo W\0\0"), buffer);
        }

        [DataTestMethod]
        [DataRow("\r\n")]
        //[DataRow("\r")] These currently fail.
        //[DataRow("\n")]
        public void ReadLine(string newLine)
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            // We specify a nonzero timeout to avoid waiting infinitely.
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.FromTicks(1)));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(newLine));

            Assert.AreEqual("Hello World!", _shellStream.ReadLine(TimeSpan.FromTicks(1)));
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.FromTicks(1)));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Second line!" + newLine + "Third line!" + newLine));

            Assert.AreEqual("Second line!", _shellStream.ReadLine(TimeSpan.FromTicks(1)));
            Assert.AreEqual("Third line!", _shellStream.ReadLine(TimeSpan.FromTicks(1)));
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.FromTicks(1)));
        }

        [DataTestMethod]
        [DataRow("\r\n")]
        [DataRow("\r")]
        [DataRow("\n")]
        public void Read_MultipleLines(string newLine)
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(newLine));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Second line!" + newLine + "Third line!" + newLine));

            Assert.AreEqual("Hello World!" + newLine + "Second line!" + newLine + "Third line!" + newLine, _shellStream.Read());
        }

        [TestMethod]
        [Ignore] // Currently returns 0 immediately
        public void Read_NonEmptyArray_OnlyReturnsZeroAfterClose()
        {
            Task closeTask = Task.Run(async () =>
            {
                // For the test to have meaning, we should be in
                // the call to Read before closing the channel.
                // Impose a short delay to make that more likely.
                await Task.Delay(50);

                _channelSessionStub.Close();
            });

            Assert.AreEqual(0, _shellStream.Read(new byte[16], 0, 16));
            Assert.AreEqual(TaskStatus.RanToCompletion, closeTask.Status);
        }

        [TestMethod]
        [Ignore] // Currently returns 0 immediately
        public void Read_EmptyArray_OnlyReturnsZeroWhenDataAvailable()
        {
            Task receiveTask = Task.Run(async () =>
            {
                // For the test to have meaning, we should be in
                // the call to Read before receiving the data.
                // Impose a short delay to make that more likely.
                await Task.Delay(50);

                _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));
            });

            Assert.AreEqual(0, _shellStream.Read(Array.Empty<byte>(), 0, 0));
            Assert.AreEqual(TaskStatus.RanToCompletion, receiveTask.Status);
        }

        [TestMethod]
        [Ignore] // Currently hangs
        public void ReadLine_NoData_ReturnsNullAfterClose()
        {
            Task closeTask = Task.Run(async () =>
            {
                await Task.Delay(50);

                _channelSessionStub.Close();
            });

            Assert.IsNull(_shellStream.ReadLine());
            Assert.AreEqual(TaskStatus.RanToCompletion, closeTask.Status);
        }

        [TestMethod]
        public void Expect()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            Assert.IsNull(_shellStream.Expect("123", TimeSpan.FromTicks(1)));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("\r\n12345"));

            // Both of these cases fail
            // Case 1 above.
            Assert.AreEqual("Hello World!\r\n123", _shellStream.Expect("123")); // Fails, returns "Hello World!\r\n12345"
            Assert.AreEqual("45", _shellStream.Read()); // Passes, but should probably fail and return ""
        }

        [TestMethod]
        public void Read_MultiByte()
        {
            _channelSessionStub.Receive(new byte[] { 0xF0 });
            _channelSessionStub.Receive(new byte[] { 0x9F });
            _channelSessionStub.Receive(new byte[] { 0x91 });
            _channelSessionStub.Receive(new byte[] { 0x8D });

            Assert.AreEqual("👍", _shellStream.Read());
        }

        [TestMethod]
        public void ReadLine_MultiByte()
        {
            _channelSessionStub.Receive(new byte[] { 0xF0 });
            _channelSessionStub.Receive(new byte[] { 0x9F });
            _channelSessionStub.Receive(new byte[] { 0x91 });
            _channelSessionStub.Receive(new byte[] { 0x8D });
            _channelSessionStub.Receive(new byte[] { 0x0D });
            _channelSessionStub.Receive(new byte[] { 0x0A });

            Assert.AreEqual("👍", _shellStream.ReadLine());
            Assert.AreEqual("", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_Regex_MultiByte()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("𐓏𐓘𐓻𐓘𐓻𐓟 𐒻𐓟"));

            Assert.AreEqual("𐓏𐓘𐓻𐓘𐓻𐓟 ", _shellStream.Expect(new Regex(@"\s")));
            Assert.AreEqual("𐒻𐓟", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_String_MultiByte()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("hello 你好"));

            Assert.AreEqual("hello 你好", _shellStream.Expect("你好"));
            Assert.AreEqual("", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_String_non_ASCII_characters()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));

            Assert.AreEqual("Hello, こ", _shellStream.Expect(new Regex(@"[^\u0000-\u007F]")));

            Assert.AreEqual("んにちは, Bonjour", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_Timeout()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Assert.IsNull(_shellStream.Expect("Hello World!", TimeSpan.FromMilliseconds(200)));

            TimeSpan elapsed = stopwatch.Elapsed;

            // Account for variance in system timer resolution.
            Assert.IsTrue(elapsed > TimeSpan.FromMilliseconds(180), elapsed.ToString());
        }

        private class ChannelSessionStub : IChannelSession
        {
            public void Receive(byte[] data)
            {
                DataReceived.Invoke(this, new ChannelDataEventArgs(channelNumber: 0, data));
            }

            public void Close()
            {
                Closed.Invoke(this, new ChannelEventArgs(channelNumber: 0));
            }

            public bool SendShellRequest()
            {
                return true;
            }

            public bool SendPseudoTerminalRequest(string environmentVariable, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues)
            {
                return true;
            }

            public void Dispose()
            {
            }

            public void Open()
            {
            }

            public event EventHandler<ChannelDataEventArgs> DataReceived;
            public event EventHandler<ChannelEventArgs> Closed;
#pragma warning disable 0067
            public event EventHandler<ExceptionEventArgs> Exception;
            public event EventHandler<ChannelExtendedDataEventArgs> ExtendedDataReceived;
            public event EventHandler<ChannelRequestEventArgs> RequestReceived;
#pragma warning restore 0067

#pragma warning disable IDE0025 // Use block body for property
#pragma warning disable IDE0022 // Use block body for method
            public uint LocalChannelNumber => throw new NotImplementedException();

            public uint LocalPacketSize => throw new NotImplementedException();

            public uint RemotePacketSize => throw new NotImplementedException();

            public bool IsOpen => throw new NotImplementedException();

            public bool SendBreakRequest(uint breakLength) => throw new NotImplementedException();

            public void SendData(byte[] data) => throw new NotImplementedException();

            public void SendData(byte[] data, int offset, int size) => throw new NotImplementedException();

            public bool SendEndOfWriteRequest() => throw new NotImplementedException();

            public bool SendEnvironmentVariableRequest(string variableName, string variableValue) => throw new NotImplementedException();

            public void SendEof() => throw new NotImplementedException();

            public bool SendExecRequest(string command) => throw new NotImplementedException();

            public bool SendExitSignalRequest(string signalName, bool coreDumped, string errorMessage, string language) => throw new NotImplementedException();

            public bool SendExitStatusRequest(uint exitStatus) => throw new NotImplementedException();

            public bool SendKeepAliveRequest() => throw new NotImplementedException();

            public bool SendLocalFlowRequest(bool clientCanDo) => throw new NotImplementedException();

            public bool SendSignalRequest(string signalName) => throw new NotImplementedException();

            public bool SendSubsystemRequest(string subsystem) => throw new NotImplementedException();

            public bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height) => throw new NotImplementedException();

            public bool SendX11ForwardingRequest(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber) => throw new NotImplementedException();
#pragma warning restore IDE0022 // Use block body for method
#pragma warning restore IDE0025 // Use block body for property
        }
    }
}
