using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Contains operation for working with SSH Shell.
    /// </summary>
    [TestClass]
    public class ShellStreamTest : TestBase
    {
        /// <summary>
        ///A test for BeginExpect
        ///</summary>
        [TestMethod()]
        public void BeginExpectTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            TimeSpan timeout = new TimeSpan(); // TODO: Initialize to an appropriate value
            AsyncCallback callback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            ExpectAction[] expectActions = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginExpect(timeout, callback, state, expectActions);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginExpect
        ///</summary>
        [TestMethod()]
        public void BeginExpectTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            ExpectAction[] expectActions = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginExpect(expectActions);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginExpect
        ///</summary>
        [TestMethod()]
        public void BeginExpectTest2()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            AsyncCallback callback = null; // TODO: Initialize to an appropriate value
            ExpectAction[] expectActions = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginExpect(callback, expectActions);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginExpect
        ///</summary>
        [TestMethod()]
        public void BeginExpectTest3()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            AsyncCallback callback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            ExpectAction[] expectActions = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginExpect(callback, state, expectActions);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EndExpect
        ///</summary>
        [TestMethod()]
        public void EndExpectTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            IAsyncResult asyncResult = null; // TODO: Initialize to an appropriate value
            target.EndExpect(asyncResult);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Expect
        ///</summary>
        [TestMethod()]
        public void ExpectTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            Regex regex = null; // TODO: Initialize to an appropriate value
            TimeSpan timeout = new TimeSpan(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Expect(regex, timeout);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Expect
        ///</summary>
        [TestMethod()]
        public void ExpectTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            Regex regex = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Expect(regex);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Expect
        ///</summary>
        [TestMethod()]
        public void ExpectTest2()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            string text = string.Empty; // TODO: Initialize to an appropriate value
            TimeSpan timeout = new TimeSpan(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Expect(text, timeout);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Expect
        ///</summary>
        [TestMethod()]
        public void ExpectTest3()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            string text = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Expect(text);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Expect
        ///</summary>
        [TestMethod()]
        public void ExpectTest4()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            ExpectAction[] expectActions = null; // TODO: Initialize to an appropriate value
            target.Expect(expectActions);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Expect
        ///</summary>
        [TestMethod()]
        public void ExpectTest5()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            TimeSpan timeout = new TimeSpan(); // TODO: Initialize to an appropriate value
            ExpectAction[] expectActions = null; // TODO: Initialize to an appropriate value
            target.Expect(timeout, expectActions);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Flush
        ///</summary>
        [TestMethod()]
        public void FlushTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            target.Flush();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Read
        ///</summary>
        [TestMethod()]
        public void ReadTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            byte[] buffer = null; // TODO: Initialize to an appropriate value
            int offset = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.Read(buffer, offset, count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Read
        ///</summary>
        [TestMethod()]
        public void ReadTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Read();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadLine
        ///</summary>
        [TestMethod()]
        public void ReadLineTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ReadLine();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadLine
        ///</summary>
        [TestMethod()]
        public void ReadLineTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            TimeSpan timeout = new TimeSpan(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ReadLine(timeout);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Seek
        ///</summary>
        [TestMethod()]
        public void SeekTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            long offset = 0; // TODO: Initialize to an appropriate value
            SeekOrigin origin = new SeekOrigin(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            actual = target.Seek(offset, origin);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SetLength
        ///</summary>
        [TestMethod()]
        public void SetLengthTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            long value = 0; // TODO: Initialize to an appropriate value
            target.SetLength(value);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            byte[] buffer = null; // TODO: Initialize to an appropriate value
            int offset = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            target.Write(buffer, offset, count);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            string text = string.Empty; // TODO: Initialize to an appropriate value
            target.Write(text);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteLine
        ///</summary>
        [TestMethod()]
        public void WriteLineTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            string line = string.Empty; // TODO: Initialize to an appropriate value
            target.WriteLine(line);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CanRead
        ///</summary>
        [TestMethod()]
        public void CanReadTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanRead;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CanSeek
        ///</summary>
        [TestMethod()]
        public void CanSeekTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanSeek;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CanWrite
        ///</summary>
        [TestMethod()]
        public void CanWriteTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanWrite;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DataAvailable
        ///</summary>
        [TestMethod()]
        public void DataAvailableTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.DataAvailable;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Length
        ///</summary>
        [TestMethod()]
        public void LengthTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            long actual;
            actual = target.Length;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Position
        ///</summary>
        [TestMethod()]
        public void PositionTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int maxLines = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream target = new ShellStream(session, terminalName, columns, rows, width, height, maxLines, terminalModeValues); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            target.Position = expected;
            actual = target.Position;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

    }
}