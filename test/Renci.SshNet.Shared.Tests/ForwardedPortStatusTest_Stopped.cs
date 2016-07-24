using System;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Renci.SshNet.Tests
{
    [TestClass]
    public class ForwardedPortStatusTest_Stopped
    {
        [TestMethod]
        public void ToStopping_ShouldReturnFalseAndNotChangeStatus()
        {
            var status = ForwardedPortStatus.Stopped;

            var actual = ForwardedPortStatus.ToStopping(ref status);

            Assert.IsFalse(actual);
            Assert.AreEqual(ForwardedPortStatus.Stopped, status);
        }

        [TestMethod]
        public void ToStarting_ShouldReturnTrueAndChangeStatusToStarting()
        {
            var status = ForwardedPortStatus.Stopped;

            var actual = ForwardedPortStatus.ToStarting(ref status);

            Assert.IsTrue(actual);
            Assert.AreEqual(ForwardedPortStatus.Starting, status);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsNull()
        {
            const ForwardedPortStatus other = null;

            var actual = ForwardedPortStatus.Stopped.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsNotInstanceOfForwardedPortStatus()
        {
            var other = new object();

            var actual = ForwardedPortStatus.Stopped.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturTrueWhenOtherIsStopped()
        {
            var other = ForwardedPortStatus.Stopped;

            var actual = ForwardedPortStatus.Stopped.Equals(other);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStopping()
        {
            var other = ForwardedPortStatus.Stopping;

            var actual = ForwardedPortStatus.Stopped.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStarted()
        {
            var other = ForwardedPortStatus.Started;

            var actual = ForwardedPortStatus.Stopped.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStarting()
        {
            var other = ForwardedPortStatus.Starting;

            var actual = ForwardedPortStatus.Stopped.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenRightIsNull()
        {
            var left = ForwardedPortStatus.Stopped;
            const ForwardedPortStatus right = null;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsNull()
        {
            const ForwardedPortStatus left = null;
            var right = ForwardedPortStatus.Stopped;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStopped()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Stopped;

            var actual = left == right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStopping()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Stopping;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStarted()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Started;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStarting()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Starting;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenRightIsNull()
        {
            var left = ForwardedPortStatus.Stopped;
            const ForwardedPortStatus right = null;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsNull()
        {
            const ForwardedPortStatus left = null;
            var right = ForwardedPortStatus.Stopped;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnFalseWhenLeftIsStoppedAndRightIsStopped()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Stopped;

            var actual = left != right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStopping()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Stopping;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStarted()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Started;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStoppedAndRightIsStarting()
        {
            var left = ForwardedPortStatus.Stopped;
            var right = ForwardedPortStatus.Starting;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void GetHashCodeShouldReturnOne()
        {
            var actual = ForwardedPortStatus.Stopped.GetHashCode();

            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public void ToStringShouldReturnStopping()
        {
            var actual = ForwardedPortStatus.Stopped.ToString();

            Assert.AreEqual("Stopped", actual);
        }
    }
}
