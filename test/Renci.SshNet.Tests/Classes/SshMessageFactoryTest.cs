using System;
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshMessageFactoryTest
    {
        private SshMessageFactory _sshMessageFactory;

        [TestInitialize]
        public void SetUp()
        {
            _sshMessageFactory = new SshMessageFactory();
        }

        [TestMethod]
        public void CreateShouldThrowSshExceptionWhenMessageIsNotEnabled()
        {
            const byte messageNumber = 60;

            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void CreateShouldThrowSshExceptionWhenMessageDoesNotExist_OutsideOfMessageNumberRange()
        {
            const byte messageNumber = 255;

            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not supported.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void CreateShouldThrowSshExceptionWhenMessageDoesNotExist_WithinMessageNumberRange()
        {
            const byte messageNumber = 5;

            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not supported.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void CreateShouldThrowSshExceptionWhenMessageIsNotActivated()
        {
            const byte messageNumber = 60;
            const string messageName = "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ";

            _sshMessageFactory.EnableAndActivateMessage(messageName);
            _sshMessageFactory.DisableAndDeactivateMessage(messageName);

            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void CreateShouldReturnMessageInstanceCorrespondingToMessageNumberWhenMessageIsEnabledAndActivated()
        {
            const byte messageNumber = 60;
            const string messageName = "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ";

            _sshMessageFactory.EnableAndActivateMessage(messageName);

            var actual = _sshMessageFactory.Create(messageNumber);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), actual.GetType());

            _sshMessageFactory.DisableAndDeactivateMessage(messageName);
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

            actual = _sshMessageFactory.Create(messageNumber);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(InformationRequestMessage), actual.GetType());
        }

        [TestMethod]
        public void DisableAndDeactivateMessageShouldThrowSshExceptionWhenAnotherMessageWithSameMessageNumberIsEnabled()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            try
            {
                _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Cannot enable message 'SSH_MSG_USERAUTH_INFO_REQUEST'. Message type 60 is already enabled for 'SSH_MSG_USERAUTH_PASSWD_CHANGEREQ'.", ex.Message);
            }

            // verify that the original message remains enabled
            var actual = _sshMessageFactory.Create(60);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), actual.GetType());
        }

        [TestMethod]
        public void DisableAndDeactivateMessageShouldNotThrowExceptionWhenMessageIsAlreadyDisabled()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            // verify that message remains disabled
            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void DisableAndDeactivateMessageShouldNotThrowExceptionWhenMessageWasNeverEnabled()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            // verify that message is disabled
            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void DisableAndDeactivateMessageShouldThrowSshExceptionWhenMessageIsNotSupported()
        {
            const string messageName = "WHATEVER";

            try
            {
                _sshMessageFactory.DisableAndDeactivateMessage("WHATEVER");
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Message '{0}' is not supported.", messageName), ex.Message);
            }
        }

        [TestMethod]
        public void DisableAndDeactivateMessageShouldThrowArgumentNullExceptionWhenMessageNameIsNull()
        {
            const string messageName = null;

            try
            {
                _sshMessageFactory.DisableAndDeactivateMessage(messageName);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("messageName", ex.ParamName);
            }
        }

        [TestMethod]
        public void EnableAndActivateMessageShouldThrowSshExceptionWhenAnotherMessageWithSameMessageNumberIsAlreadyEnabled()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            try
            {
                _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Cannot enable message 'SSH_MSG_USERAUTH_INFO_REQUEST'. Message type 60 is already enabled for 'SSH_MSG_USERAUTH_PASSWD_CHANGEREQ'.", ex.Message);
            }

            // verify that the original message remains enabled
            var actual = _sshMessageFactory.Create(60);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), actual.GetType());
        }

        [TestMethod]
        public void EnableAndActivateMessageShouldNotThrowExceptionWhenMessageIsAlreadyEnabled()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            var actual = _sshMessageFactory.Create(60);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), actual.GetType());
        }

        [TestMethod]
        public void EnableAndActivateMessageShouldThrowSshExceptionWhenMessageIsNotSupported()
        {
            const string messageName = "WHATEVER";

            try
            {
                _sshMessageFactory.EnableAndActivateMessage("WHATEVER");
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Message '{0}' is not supported.", messageName), ex.Message);
            }
        }

        [TestMethod]
        public void EnableAndActivateMessageShouldThrowArgumentNullExceptionWhenMessageNameIsNull()
        {
            const string messageName = null;

            try
            {
                _sshMessageFactory.EnableAndActivateMessage(messageName);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("messageName", ex.ParamName);
            }
        }

        [TestMethod]
        public void DisableNonKeyExchangeMessagesShouldDisableNonKeyExchangeMessages()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);

            // verify that message is disabled
            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void DisableNonKeyExchangeMessagesShouldNotDisableKeyExchangeMessages()
        {
            const byte messageNumber = 21;

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_NEWKEYS");
            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);

            // verify that message remains enabled
            var actual = _sshMessageFactory.Create(messageNumber);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(NewKeysMessage), actual.GetType());
        }

        [TestMethod]
        public void EnableActivatedMessagesShouldEnableMessagesThatWereEnabledPriorToInvokingDisableNonKeyExchangeMessages()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
            _sshMessageFactory.EnableActivatedMessages();

            var actual = _sshMessageFactory.Create(messageNumber);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), actual.GetType());
        }

        [TestMethod]
        public void EnableActivatedMessagesShouldNotEnableMessagesThatWereDisabledPriorToInvokingDisableNonKeyExchangeMessages()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
            _sshMessageFactory.EnableActivatedMessages();

            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void EnableActivatedMessagesShouldNotEnableMessagesThatWereDisabledAfterInvokingDisableNonKeyExchangeMessages()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
            _sshMessageFactory.DisableAndDeactivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.EnableActivatedMessages();

            try
            {
                _sshMessageFactory.Create(messageNumber);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid in the current context.", messageNumber), ex.Message);
            }
        }

        [TestMethod]
        public void EnableActivatedMessagesShouldThrowSshExceptionWhenAnothersMessageWithSameMessageNumberWasEnabledAfterInvokingDisableNonKeyExchangeMessages()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

            try
            {
                _sshMessageFactory.EnableActivatedMessages();
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Cannot enable message 'SSH_MSG_USERAUTH_PASSWD_CHANGEREQ'. Message type 60 is already enabled for 'SSH_MSG_USERAUTH_INFO_REQUEST'.", ex.Message);
            }
        }

        [TestMethod]
        public void EnableActivatedMessagesShouldLeaveMessagesEnabledThatWereEnabledAfterInvokingDisableNonKeyExchangeMessages()
        {
            const byte messageNumber = 60;

            _sshMessageFactory.DisableNonKeyExchangeMessages(strict: false);
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.EnableActivatedMessages();

            var actual = _sshMessageFactory.Create(messageNumber);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), actual.GetType());
        }

        [TestMethod]
        public void HighestMessageNumberShouldCorrespondWithHighestSupportedMessageNumber()
        {
            var highestSupportMessageNumber = SshMessageFactory.AllMessages.Max(m => m.Number);

            Assert.AreEqual(highestSupportMessageNumber, SshMessageFactory.HighestMessageNumber);
        }

        [TestMethod]
        public void TotalMessageCountShouldBeTotalNumberOfSupportedMessages()
        {
            var totalNumberOfSupportedMessages = SshMessageFactory.AllMessages.Length;

            Assert.AreEqual(totalNumberOfSupportedMessages, SshMessageFactory.TotalMessageCount);
        }
    }
}
