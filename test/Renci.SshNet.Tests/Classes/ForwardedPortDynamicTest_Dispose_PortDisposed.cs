using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest_Dispose_PortDisposed
    {
        private ForwardedPortDynamic _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _forwardedPort?.Dispose();
            _forwardedPort = null;
        }

        protected void Arrange()
        {
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();

            _forwardedPort = new ForwardedPortDynamic("host", 22);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Dispose();
        }

        protected void Act()
        {
            _forwardedPort.Dispose();
        }

        [TestMethod]
        public void ClosingShouldNotHaveFired()
        {
            Assert.IsEmpty(_closingRegister);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.IsEmpty(_exceptionRegister);
        }
    }
}
