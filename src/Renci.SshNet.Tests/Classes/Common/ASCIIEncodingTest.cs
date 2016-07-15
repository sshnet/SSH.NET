using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ASCIIEncodingTest : TestBase
    {
        private Random _random;
        private Encoding _ascii;

        [TestInitialize]
        public void SetUp()
        {
            _random = new Random();
            _ascii = SshData.Ascii;
        }

        [TestMethod]
        public void GetByteCount_Chars()
        {
            var chars = new[] { 'B', 'e', 'l', 'g', 'i', 'u', 'm' };

            var actual = _ascii.GetByteCount(chars);

            Assert.AreEqual(chars.Length, actual);
        }

        [TestMethod]
        public void GetBytes_CharArray()
        {
            var chars = new[] {'B', 'e', 'l', 'g', 'i', 'u', 'm'};

            var actual = _ascii.GetBytes(chars);

            Assert.IsNotNull(actual);
            Assert.AreEqual(7, actual.Length);
            Assert.AreEqual(0x42, actual[0]);
            Assert.AreEqual(0x65, actual[1]);
            Assert.AreEqual(0x6c, actual[2]);
            Assert.AreEqual(0x67, actual[3]);
            Assert.AreEqual(0x69, actual[4]);
            Assert.AreEqual(0x75, actual[5]);
            Assert.AreEqual(0x6d ,actual[6]);
        }

        [TestMethod]
        public void GetCharCount_Bytes()
        {
            var bytes = new byte[] { 0x42, 0x65, 0x6c, 0x67, 0x69, 0x75, 0x6d };

            var actual = _ascii.GetCharCount(bytes);

            Assert.AreEqual(bytes.Length, actual);
        }

        [TestMethod]
        public void GetChars_Bytes()
        {
            var bytes = new byte[] {0x42, 0x65, 0x6c, 0x67, 0x69, 0x75, 0x6d};

            var actual = _ascii.GetChars(bytes);

            Assert.AreEqual("Belgium", new string(actual));
        }

        [TestMethod]
        public void GetChars_Bytes_DefaultFallback()
        {
            var bytes = new byte[] { 0x42, 0x65, 0x6c, 0x80, 0x69, 0x75, 0x6d };

            var actual = _ascii.GetChars(bytes);

            Assert.AreEqual("Bel?ium", new string(actual));
        }

        [TestMethod]
        public void GetMaxByteCount_ShouldReturnCharCountPlusOneWhenCharCountIsNonNegative()
        {
            var charCount = _random.Next(0, 20000);

            var actual = _ascii.GetMaxByteCount(charCount);

            Assert.AreEqual(++charCount, actual);
        }

        [TestMethod]
        public void GetMaxByteCount_ShouldThrowArgumentOutOfRangeExceptionWhenCharCountIsNegative()
        {
            var charCount = _random.Next(-5000, -1);

            try
            {
                var actual = _ascii.GetMaxByteCount(charCount);
                Assert.Fail(actual.ToString(CultureInfo.InvariantCulture));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("charCount", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetMaxCharCount_ShouldReturnByteCountWhenByteCountIsNonNegative()
        {
            var byteCount = _random.Next(0, 20000);

            var actual = _ascii.GetMaxCharCount(byteCount);

            Assert.AreEqual(byteCount, actual);
        }

        [TestMethod]
        public void GetMaxCharCount_ShouldThrowArgumentOutOfRangeExceptionWhenByteCountIsNegative()
        {
            var byteCount = _random.Next(-5000, -1);

            try
            {
                var actual = _ascii.GetMaxCharCount(byteCount);
                Assert.Fail(actual.ToString(CultureInfo.InvariantCulture));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("byteCount", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetPreamble()
        {
            var actual = _ascii.GetPreamble();

            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void IsSingleByte()
        {
            Assert.IsTrue(_ascii.IsSingleByte);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void GetBytes_Performance()
        {
            const string input = "eererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqseererzfdfdsfsfsfsqdqs";
            const int loopCount = 10000000;
            var result = new byte[input.Length];

            var corefxAscii = new System.Text.ASCIIEncoding();
            var sshAscii = _ascii;

            var stopWatch = new Stopwatch();

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch.Start();

            for (var i = 0; i < loopCount; i++)
            {
                corefxAscii.GetBytes(input, 0, input.Length, result, 0);
            }

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);

            stopWatch.Reset();

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch.Start();

            for (var i = 0; i < loopCount; i++)
            {
                sshAscii.GetBytes(input, 0, input.Length, result, 0);
            }

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void GetChars_Performance()
        {
            var input = new byte[2000];
            new Random().NextBytes(input);
            const int loopCount = 100000;

            var corefxAscii = new System.Text.ASCIIEncoding();
            var sshAscii = _ascii;

            var stopWatch = new Stopwatch();

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch.Start();

            for (var i = 0; i < loopCount; i++)
            {
                var actual = corefxAscii.GetChars(input);
            }

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);

            stopWatch.Reset();

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch.Start();

            for (var i = 0; i < loopCount; i++)
            {
                var actual = sshAscii.GetChars(input);
            }

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }

    }
}