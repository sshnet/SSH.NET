using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshCommandTest_EndExecute_AsyncResultIsNull : TestBase
    {
        private Mock<ISession> _sessionMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommand;
        private IAsyncResult _asyncResult;
        private ArgumentNullException _actualException;

        protected override void OnInit()
        {
            base.OnInit();

            Arrange();
            Act();
        }

        private void Arrange()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
            _encoding = Encoding.UTF8;
            _asyncResult = null;

            _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
        }

        private void Act()
        {
            try
            {
                _sshCommand.EndExecute(_asyncResult);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void EndExecuteShouldHaveThrownArgumentNullException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("asyncResult", _actualException.ParamName);
        }
    }
}
