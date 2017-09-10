using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class ExtensionsTest_Take_Count
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            const byte[] value = null;
            const int count = 0;

            try
            {
                Extensions.Take(value, count);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldReturnEmptyByteArrayWhenCountIsZero()
        {
            var value = CreateBuffer(16);
            const int count = 0;

            var actual = Extensions.Take(value, count);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void ShouldReturnValueWhenCountIsEqualToLengthOfValue()
        {
            var value = CreateBuffer(16);
            var count = value.Length;

            var actual = Extensions.Take(value, count);

            Assert.IsNotNull(actual);
            Assert.AreSame(value, actual);
        }

        [TestMethod]
        public void ShouldReturnLeadingBytesWhenCountIsLessThanLengthOfValue()
        {
            var value = CreateBuffer(16);
            const int count = 5;

            var actual = Extensions.Take(value, count);

            Assert.IsNotNull(actual);
            Assert.AreEqual(count, actual.Length);
            Assert.AreEqual(value[0], actual[0]);
            Assert.AreEqual(value[1], actual[1]);
            Assert.AreEqual(value[2], actual[2]);
            Assert.AreEqual(value[3], actual[3]);
            Assert.AreEqual(value[4], actual[4]);
        }

        [TestMethod]
        public void ShouldThrowArgumentExceptionWhenCountIsGreaterThanLengthOfValue()
        {
            var value = CreateBuffer(16);
            var count = value.Length + 1;

            try
            {
                Extensions.Take(value, count);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_All()
        {
            var value = CreateBuffer(50000);
            var count = value.Length;
            const int runs = 10000;

            Performance(value, count, runs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_LargeCount()
        {
            var value = CreateBuffer(50000);
            const int count = 40000;
            const int runs = 1000000;

            Performance(value, count, runs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_SmallCount()
        {
            var value = CreateBuffer(50000);
            const int count = 50;
            const int runs = 1000000;

            Performance(value, count, runs);
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void Performance_LargeArray_ZeroCount()
        {
            var value = CreateBuffer(50000);
            const int count = 0;
            const int runs = 1000000;

            Performance(value, count, runs);
        }

        private static void Performance(byte[] value, int count, int runs)
        {
            var stopWatch = new Stopwatch();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopWatch.Start();

            for (var i = 0; i < runs; i++)
            {
                var result = Extensions.Take(value, count);
                var resultLength = result.Length;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);

            stopWatch.Reset();
            stopWatch.Start();

            for (var i = 0; i < runs; i++)
            {
                var result = Enumerable.Take(value, count);
                var resultLength = result.ToArray().Length;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }

        private byte[] CreateBuffer(int length)
        {
            var buffer = new byte[length];
            _random.NextBytes(buffer);
            return buffer;
        }
    }
}
