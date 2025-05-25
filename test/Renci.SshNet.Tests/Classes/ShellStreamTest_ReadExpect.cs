using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ShellStreamTest_ReadExpect
    {
        private const int BufferSize = 1024;
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
                bufferSize: BufferSize);
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

#if NET
        [TestMethod]
        public void Read_Bytes_Span()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            byte[] buffer = new byte[12];

            Assert.AreEqual(7, _shellStream.Read(buffer.AsSpan(3, 7)));
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("\0\0\0Hello W\0\0"), buffer);

            Assert.AreEqual(5, _shellStream.Read(buffer));
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("orld!llo W\0\0"), buffer);
        }
#endif

        [TestMethod]
        public void Channel_DataReceived_MoreThanBufferSize()
        {
            // Test buffer resizing
            byte[] expectedData = CryptoAbstraction.GenerateRandom(BufferSize * 3);
            _channelSessionStub.Receive(expectedData);

            byte[] actualData = new byte[expectedData.Length + 1];

            Assert.AreEqual(expectedData.Length, _shellStream.Read(actualData, 0, actualData.Length));
            CollectionAssert.AreEqual(expectedData, actualData.Take(expectedData.Length));
        }

        [DataTestMethod]
        [DataRow("\r\n")]
        [DataRow("\r")]
        [DataRow("\n")]
        public void ReadLine(string newLine)
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            // We specify a timeout to avoid waiting infinitely.
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.Zero));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(newLine));

            Assert.AreEqual("Hello World!", _shellStream.ReadLine(TimeSpan.Zero));
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.Zero));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Second line!" + newLine + "Third line!" + newLine));

            Assert.AreEqual("Second line!", _shellStream.ReadLine(TimeSpan.Zero));
            Assert.AreEqual("Third line!", _shellStream.ReadLine(TimeSpan.Zero));
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.Zero));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Last line!")); // no newLine at the end

            Assert.IsNull(_shellStream.ReadLine(TimeSpan.Zero));

            _channelSessionStub.Close();

            Assert.AreEqual("Last line!", _shellStream.ReadLine(TimeSpan.Zero));
        }

        [TestMethod]
        public void ReadLine_DifferentTerminators()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello\rWorld!\nWhat's\r\ngoing\n\ron?\n"));

            Assert.AreEqual("Hello", _shellStream.ReadLine());
            Assert.AreEqual("World!", _shellStream.ReadLine());
            Assert.AreEqual("What's", _shellStream.ReadLine());
            Assert.AreEqual("going", _shellStream.ReadLine());
            Assert.AreEqual("", _shellStream.ReadLine());
            Assert.AreEqual("on?", _shellStream.ReadLine());
            Assert.IsNull(_shellStream.ReadLine(TimeSpan.Zero));
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
        public async Task Read_NonEmptyArray_OnlyReturnsZeroAfterClose()
        {
            Task<int> readTask = _shellStream.ReadAsync(new byte[16], 0, 16);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            _channelSessionStub.Close();

            Assert.AreEqual(0, await readTask);
        }

        [TestMethod]
        public async Task Read_EmptyArray_OnlyReturnsZeroWhenDataAvailable()
        {
            Task<int> readTask = _shellStream.ReadAsync(Array.Empty<byte>(), 0, 0);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));

            Assert.AreEqual(0, await readTask);
        }

#if NET
        [TestMethod]
        public async Task Read_EmptySpan_OnlyReturnsZeroWhenDataAvailable()
        {
            ValueTask<int> readTask = _shellStream.ReadAsync(Memory<byte>.Empty);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));

            Assert.AreEqual(0, await readTask);
        }
#endif

        [TestMethod]
        public void Expect()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello "));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("World!"));

            Assert.IsNull(_shellStream.Expect("123", TimeSpan.Zero));

            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("\r\n12345"));

            Assert.AreEqual("Hello World!\r\n123", _shellStream.Expect("123"));
            Assert.AreEqual("45", _shellStream.Read());
        }

        [TestMethod]
        public void Read_AfterDispose_StillWorks()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello World!"));

            _shellStream.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
            _shellStream.Dispose(); // Check that multiple Dispose is OK.
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.IsTrue(_shellStream.CanRead);
            Assert.AreEqual("Hello World!", _shellStream.ReadLine());
            Assert.IsNull(_shellStream.ReadLine());
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
        public void Expect_Regex_non_ASCII_characters()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));

            Assert.AreEqual("Hello, こ", _shellStream.Expect(new Regex(@"[^\u0000-\u007F]")));

            Assert.AreEqual("んにちは, Bonjour", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_String_LargeExpect()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('a', 100)));
            for (var i = 0; i < 10; i++)
            {
                _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('b', 100)));
            }
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('c', 100)));

            Assert.AreEqual($"{new string('a', 100)}{new string('b', 1000)}Hello, こんにちは, Bonjour", _shellStream.Expect($"{new string('b', 1000)}Hello, こんにちは, Bonjour"));

            Assert.AreEqual($"{new string('c', 100)}", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_String_WithLookback()
        {
            const string expected = "ccccc";

            // Prime buffer
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string(' ', BufferSize)));

            // Test data
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('a', 100)));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('b', 100)));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(expected));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('d', 100)));
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes(new string('e', 100)));

            // Expected result
            var expectedResult = $"{new string(' ', BufferSize)}{new string('a', 100)}{new string('b', 100)}{expected}";
            var expectedRead = $"{new string('d', 100)}{new string('e', 100)}";

            Assert.AreEqual(expectedResult, _shellStream.Expect(expected, TimeSpan.Zero, lookback: 250));

            Assert.AreEqual(expectedRead, _shellStream.Read());
        }

        [TestMethod]
        public void Expect_Regex_WithLookback()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("0123456789"));

            Assert.AreEqual("01234567", _shellStream.Expect(new Regex(@"\d"), TimeSpan.Zero, lookback: 3));

            Assert.AreEqual("89", _shellStream.Read());
        }

        [TestMethod]
        public void Expect_Regex_WithLookback_non_ASCII_characters()
        {
            _channelSessionStub.Receive(Encoding.UTF8.GetBytes("Hello, こんにちは, Bonjour"));

            Assert.AreEqual("Hello, こんにち", _shellStream.Expect(new Regex(@"[^\u0000-\u007F]"), TimeSpan.Zero, lookback: 11));

            Assert.AreEqual("は, Bonjour", _shellStream.Read());
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

            public uint RemoteChannelNumber => throw new NotImplementedException();

            public uint RemotePacketSize => throw new NotImplementedException();

            public bool IsOpen => throw new NotImplementedException();

            public bool SendBreakRequest(uint breakLength) => throw new NotImplementedException();

            public void SendData(byte[] data) => throw new NotImplementedException();

            public void SendData(byte[] data, int offset, int size) => throw new NotImplementedException();

            public bool SendEndOfWriteRequest() => throw new NotImplementedException();

            public bool SendEnvironmentVariableRequest(string variableName, string variableValue) => throw new NotImplementedException();

            public void SendEof() => throw new NotImplementedException();

            public bool SendExecRequest(string command) => throw new NotImplementedException();

            public bool SendKeepAliveRequest() => throw new NotImplementedException();

            public void SendLocalFlowRequest(bool clientCanDo) => throw new NotImplementedException();

            public bool SendSignalRequest(string signalName) => throw new NotImplementedException();

            public bool SendSubsystemRequest(string subsystem) => throw new NotImplementedException();

            public void SendWindowChangeRequest(uint columns, uint rows, uint width, uint height) => throw new NotImplementedException();

            public bool SendX11ForwardingRequest(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber) => throw new NotImplementedException();
#pragma warning restore IDE0022 // Use block body for method
#pragma warning restore IDE0025 // Use block body for property
        }
    }
}
