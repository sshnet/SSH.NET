using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class ExtensionsTest_IsEqualTo_ByteArray
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenLeftIsNull()
        {
            const byte[] left = null;
            var right = CreateBuffer(1);

            try
            {
                Extensions.IsEqualTo(left, right);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("left", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenRightIsNull()
        {
            var left = CreateBuffer(1);
            const byte[] right = null;

            try
            {
                Extensions.IsEqualTo(left, right);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("right", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenLeftAndRightAreNull()
        {
            const byte[] left = null;
            const byte[] right = null;

            try
            {
                Extensions.IsEqualTo(left, right);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("left", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldReturnFalseWhenLeftIsNotEqualToRight()
        {
            Assert.IsFalse(Extensions.IsEqualTo(new byte[] {0x0a}, new byte[] {0x0a, 0x0d}));
            Assert.IsFalse(Extensions.IsEqualTo(new byte[] {0x0a, 0x0d}, new byte[] {0x0a}));
            Assert.IsFalse(Extensions.IsEqualTo(new byte[0], new byte[] { 0x0a }));
            Assert.IsFalse(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[0]));
        }

        [TestMethod]
        public void ShouldReturnTrueWhenLeftIsEqualToRight()
        {
            Assert.IsTrue(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[] { 0x0a, 0x0d }));
            Assert.IsTrue(Extensions.IsEqualTo(new byte[0], new byte[0]));
        }

        [TestMethod]
        public void ShouldReturnTrueWhenLeftIsSameAsRight()
        {
            var left = new byte[] { 0x0d, 0x0d };

            Assert.IsTrue(Extensions.IsEqualTo(left, left));
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_Equal()
        {
            var buffer = CreateBuffer(50000);
            var left = buffer.Concat(new byte[] {0x0a});
            var right = buffer.Concat(new byte[] {0x0a});
            const int runs = 10000;

            Performance(left, right, runs);
        }
        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_NotEqual_DifferentLength()
        {
            var left = CreateBuffer(50000);
            var right = left.Concat(new byte[] {0x0a});
            const int runs = 10000;

            Performance(left, right, runs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_NotEqual_SameLength()
        {
            var buffer = CreateBuffer(50000);
            var left = buffer.Concat(new byte[] {0x0a});
            var right = buffer.Concat(new byte[] {0x0b});
            const int runs = 10000;

            Performance(left, right, runs);
        }

        [TestMethod]
        [TestCategory("LongRunning")]
        [TestCategory("Performance")]
        public void Performance_LargeArray_Same()
        {
            var left = CreateBuffer(50000);
            var right = left.Concat(new byte[] {0x0a});
            const int runs = 10000;

            Performance(left, right, runs);
        }

        private static void Performance(byte[] left, byte[] right, int runs)
        {
            var stopWatch = new Stopwatch();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopWatch.Start();

            for (var i = 0; i < runs; i++)
            {
                Extensions.IsEqualTo(left, right);
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
                var result = System.Linq.Enumerable.SequenceEqual(left, right);
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
