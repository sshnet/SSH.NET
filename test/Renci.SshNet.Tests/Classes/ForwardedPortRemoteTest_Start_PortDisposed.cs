using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortRemoteTest_Start_PortDisposed
    {
        private ForwardedPortRemote _forwardedPort;
        private IPEndPoint _bindEndpoint;
        private IPEndPoint _remoteEndpoint;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private ObjectDisposedException _actualException;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_forwardedPort != null)
            {
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }
        }

        protected void Arrange()
        {
            var random = new Random();
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _bindEndpoint = new IPEndPoint(IPAddress.Any, random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _forwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port);

            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Dispose();
        }

        protected void Act()
        {
            try
            {
                _forwardedPort.Start();
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void StartShouldThrowObjectDisposedException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreEqual(_forwardedPort.GetType().FullName, _actualException.ObjectName);
        }

        [TestMethod]
        public void ClosingShouldNotHaveFired()
        {
            Assert.AreEqual(0, _closingRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.AreEqual(0, _exceptionRegister.Count);
        }
    }
}
