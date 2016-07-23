using System;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Renci.SshNet.Tests
{
    [TestClass]
    public class ForwardedPortStatusTest_Stopping
    {
        [TestMethod]
        public void ToStopping_ShouldReturnFalseAndNotChangeStatus()
        {
            var status = ForwardedPortStatus.Stopping;

            var actual = ForwardedPortStatus.ToStopping(ref status);

            Assert.IsFalse(actual);
            Assert.AreEqual(ForwardedPortStatus.Stopping, status);
        }

        [TestMethod]
        public void ToStarting_ShouldThrowInvalidOperationExceptionAndNotChangeStatus()
        {
            var status = ForwardedPortStatus.Stopping;

            try
            {
                ForwardedPortStatus.ToStarting(ref status);
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(
                    string.Format("Forwarded port cannot transition from '{0}' to '{1}'.",
                                  ForwardedPortStatus.Stopping,
                                  ForwardedPortStatus.Starting),
                    ex.Message);
            }

            Assert.AreEqual(ForwardedPortStatus.Stopping, status);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsNull()
        {
            const ForwardedPortStatus other = null;

            var actual = ForwardedPortStatus.Stopping.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsNotInstanceOfForwardedPortStatus()
        {
            var other = new object();

            var actual = ForwardedPortStatus.Stopping.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStopped()
        {
            var other = ForwardedPortStatus.Stopped;

            var actual = ForwardedPortStatus.Stopping.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnTrueWhenOtherIsStopping()
        {
            var other = ForwardedPortStatus.Stopping;

            var actual = ForwardedPortStatus.Stopping.Equals(other);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStarted()
        {
            var other = ForwardedPortStatus.Started;

            var actual = ForwardedPortStatus.Stopping.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStarting()
        {
            var other = ForwardedPortStatus.Starting;

            var actual = ForwardedPortStatus.Stopping.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenRightIsNull()
        {
            var left = ForwardedPortStatus.Stopping;
            const ForwardedPortStatus right = null;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsNull()
        {
            const ForwardedPortStatus left = null;
            var right = ForwardedPortStatus.Stopping;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStopped()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Stopped;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStopping()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Stopping;

            var actual = left == right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStarted()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Started;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStarting()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Starting;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenRightIsNull()
        {
            var left = ForwardedPortStatus.Stopping;
            const ForwardedPortStatus right = null;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsNull()
        {
            const ForwardedPortStatus left = null;
            var right = ForwardedPortStatus.Stopping;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStopped()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Stopped;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnFalseWhenLeftIsStoppingAndRightIsStopping()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Stopping;

            var actual = left != right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStarted()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Started;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppingAndRightIsStarting()
        {
            var left = ForwardedPortStatus.Stopping;
            var right = ForwardedPortStatus.Starting;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void GetHashCodeShouldReturnTwo()
        {
            var actual = ForwardedPortStatus.Stopping.GetHashCode();

            Assert.AreEqual(2, actual);
        }

        [TestMethod]
        public void ToStringShouldReturnStopping()
        {
            var actual = ForwardedPortStatus.Stopping.ToString();

            Assert.AreEqual("Stopping", actual);
        }
    }
}
