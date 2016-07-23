using System;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Renci.SshNet.Tests
{
    [TestClass]
    public class ForwardedPortStatusTest_Started
    {
        [TestMethod]
        public void ToStopping_ShouldReturnTrueAndChangeStatusToStopping()
        {
            var status = ForwardedPortStatus.Started;

            var actual = ForwardedPortStatus.ToStopping(ref status);

            Assert.IsTrue(actual);
            Assert.AreEqual(ForwardedPortStatus.Stopping, status);
        }

        [TestMethod]
        public void ToStarting_ShouldReturnFalseAndNotChangeStatus()
        {
            var status = ForwardedPortStatus.Started;

            var actual = ForwardedPortStatus.ToStarting(ref status);

            Assert.IsFalse(actual);
            Assert.AreEqual(ForwardedPortStatus.Started, status);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsNull()
        {
            const ForwardedPortStatus other = null;

            var actual = ForwardedPortStatus.Started.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsNotInstanceOfForwardedPortStatus()
        {
            var other = new object();

            var actual = ForwardedPortStatus.Started.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStopped()
        {
            var other = ForwardedPortStatus.Stopped;

            var actual = ForwardedPortStatus.Started.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStopping()
        {
            var other = ForwardedPortStatus.Stopping;

            var actual = ForwardedPortStatus.Started.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnTrueWhenOtherIsStarted()
        {
            var other = ForwardedPortStatus.Started;

            var actual = ForwardedPortStatus.Started.Equals(other);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Equals_ShouldReturnFalseWhenOtherIsStarting()
        {
            var other = ForwardedPortStatus.Starting;

            var actual = ForwardedPortStatus.Started.Equals(other);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenRightIsNull()
        {
            var left = ForwardedPortStatus.Started;
            const ForwardedPortStatus right = null;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsNull()
        {
            const ForwardedPortStatus left = null;
            var right = ForwardedPortStatus.Started;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartedAndRightIsStopped()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Stopped;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartedAndRightIsStopping()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Stopping;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartedAndRightIsStarted()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Started;

            var actual = left == right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void EqualityOperator_ShouldReturnFalseWhenLeftIsStartedAndRightIsStarting()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Starting;

            var actual = left == right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenRightIsNull()
        {
            var left = ForwardedPortStatus.Started;
            const ForwardedPortStatus right = null;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsNull()
        {
            const ForwardedPortStatus left = null;
            var right = ForwardedPortStatus.Started;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartedAndRightIsStopped()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Stopped;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartedAndRightIsStopping()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Stopping;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartedAndRightIsStarted()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Started;

            var actual = left != right;

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void InEqualityOperator_ShouldReturnTrueWhenLeftIsStartedAndRightIsStarting()
        {
            var left = ForwardedPortStatus.Started;
            var right = ForwardedPortStatus.Starting;

            var actual = left != right;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void GetHashCodeShouldReturnThree()
        {
            var actual = ForwardedPortStatus.Started.GetHashCode();

            Assert.AreEqual(3, actual);
        }

        [TestMethod]
        public void ToStringShouldReturnStarted()
        {
            var actual = ForwardedPortStatus.Started.ToString();

            Assert.AreEqual("Started", actual);
        }
    }
}
