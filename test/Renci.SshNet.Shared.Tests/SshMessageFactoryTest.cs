#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;

#endif

namespace Renci.SshNet.Tests
{
    [TestClass]
    public class SshMessageFactoryTest
    {
        private SshMessageFactory _sshMessageFactory;
        private SshMessageFactoryOriginal _sshMessageFactoryOriginal;

        [TestInitialize]
        public void SetUp()
        {
            _sshMessageFactory = new SshMessageFactory();
            _sshMessageFactoryOriginal = new SshMessageFactoryOriginal();
        }

        [TestMethod]
        public void EnableInformationRequestMessage()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
            var message = _sshMessageFactory.Create(60);
            Assert.AreEqual(typeof(InformationRequestMessage), message.GetType());
        }

        [TestMethod]
        public void EnablePasswordChangeRequiredMessage()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            var message = _sshMessageFactory.Create(60);
            Assert.AreEqual(typeof(PasswordChangeRequiredMessage), message.GetType());
        }

        [TestMethod]
        public void Di()
        {
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_SERVICE_ACCEPT");

            _sshMessageFactory.DisableNonKeyExchangeMessages();

            try
            {
                _sshMessageFactory.Create(6); // SSH_MSG_SERVICE_ACCEPT
                Assert.Fail();
            }
            catch (SshException)
            {
            }

            try
            {
                _sshMessageFactory.Create(60);
                    // SSH_MSG_USERAUTH_PASSWD_CHANGEREQ or SSH_MSG_USERAUTH_INFO_REQUEST or SSH_MSG_USERAUTH_PK_OK
                Assert.Fail();
            }
            catch (SshException)
            {
            }

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
            var message = _sshMessageFactory.Create(60);
            Assert.AreEqual(typeof(InformationRequestMessage), message.GetType());

            _sshMessageFactory.EnableActivatedMessages();

            var message2 = _sshMessageFactory.Create(60);
            Assert.AreEqual(typeof(InformationRequestMessage), message2.GetType());
        }

        [TestMethod]
        public void Performance_Ctor()
        {
            const int runCount = 100000;

            // warm-up
            for (var i = 0; i < 3; i++)
            {
                var sshMessageFactory = new SshMessageFactory();
                var sshMessageFactoryOriginal = new SshMessageFactoryOriginal();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
            {
                var sshMessageFactory = new SshMessageFactory();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
            {
                var sshMessageFactory = new SshMessageFactoryOriginal();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Performance_Create()
        {
            const int runCount = 10000000;
            const string messageName = "SSH_MSG_CHANNEL_CLOSE";

            _sshMessageFactory.EnableAndActivateMessage(messageName);
            _sshMessageFactoryOriginal.EnableAndActivateMessage(messageName);

            // warm-up
            for (var i = 0; i < 3; i++)
            {
                _sshMessageFactory.Create(97);
                _sshMessageFactoryOriginal.Create(97);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
            {
                var msg = _sshMessageFactory.Create(97);
                if (msg == null)
                    Console.WriteLine();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
            {
                var msg = _sshMessageFactoryOriginal.Create(97);
                if (msg == null)
                    Console.WriteLine();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Performance_EnableAndActivateMessage()
        {
            const int runCount = 1000000;
            const string messageName = "SSH_MSG_CHANNEL_CLOSE";

            // warm-up
            for (var i = 0; i < 3; i++)
            {
                _sshMessageFactory.EnableAndActivateMessage(messageName);
                _sshMessageFactoryOriginal.EnableAndActivateMessage(messageName);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
                _sshMessageFactory.EnableAndActivateMessage(messageName);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
                _sshMessageFactoryOriginal.EnableAndActivateMessage(messageName);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Performance_DisableAndDeactivateMessage()
        {
            const int runCount = 1000000;
            const string messageName = "SSH_MSG_CHANNEL_CLOSE";

            // warm-up
            for (var i = 0; i < 3; i++)
            {
                _sshMessageFactory.DisableAndDeactivateMessage(messageName);
                _sshMessageFactoryOriginal.DisableAndDeactivateMessage(messageName);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
                _sshMessageFactory.DisableAndDeactivateMessage(messageName);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
                _sshMessageFactoryOriginal.DisableAndDeactivateMessage(messageName);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Performance_DisableNonKeyExchangeMessages()
        {
            const int runCount = 1000000;

            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_USERAUTH_BANNER");
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_DEBUG");
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_UNIMPLEMENTED");
            _sshMessageFactory.EnableAndActivateMessage("SSH_MSG_SERVICE_ACCEPT");

            _sshMessageFactoryOriginal.EnableAndActivateMessage("SSH_MSG_USERAUTH_BANNER");
            _sshMessageFactoryOriginal.EnableAndActivateMessage("SSH_MSG_DEBUG");
            _sshMessageFactoryOriginal.EnableAndActivateMessage("SSH_MSG_UNIMPLEMENTED");
            _sshMessageFactoryOriginal.EnableAndActivateMessage("SSH_MSG_SERVICE_ACCEPT");

            // warm-up
            for (var i = 0; i < 3; i++)
            {
                _sshMessageFactory.DisableNonKeyExchangeMessages();
                _sshMessageFactory.EnableActivatedMessages();

                _sshMessageFactoryOriginal.DisableNonKeyExchangeMessages();
                _sshMessageFactoryOriginal.EnableActivatedMessages();
            }

            //Console.WriteLine("Starting test");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
            {
                _sshMessageFactory.DisableNonKeyExchangeMessages();
                _sshMessageFactory.EnableActivatedMessages();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            stopwatch.Start();

            for (var i = 0; i < runCount; i++)
            {
                _sshMessageFactoryOriginal.DisableNonKeyExchangeMessages();
                _sshMessageFactoryOriginal.EnableActivatedMessages();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        internal class SshMessageFactoryOriginal
        {
            private readonly IEnumerable<MessageMetadata> _messagesMetadata;

            public SshMessageFactoryOriginal()
            {
                _messagesMetadata = new[]
                {
                    new MessageMetadata {Name = "SSH_MSG_NEWKEYS", Number = 21, Type = typeof(NewKeysMessage)},
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_REQUEST_FAILURE",
                        Number = 82,
                        Type = typeof(RequestFailureMessage)
                    },
                    new MessageMetadata {Name = "SSH_MSG_KEXINIT", Number = 20, Type = typeof(KeyExchangeInitMessage)},
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_OPEN_FAILURE",
                        Number = 92,
                        Type = typeof(ChannelOpenFailureMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_FAILURE",
                        Number = 100,
                        Type = typeof(ChannelFailureMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_EXTENDED_DATA",
                        Number = 95,
                        Type = typeof(ChannelExtendedDataMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_DATA",
                        Number = 94,
                        Type = typeof(ChannelDataMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_REQUEST",
                        Number = 50,
                        Type = typeof(RequestMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_REQUEST",
                        Number = 98,
                        Type = typeof(ChannelRequestMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_BANNER",
                        Number = 53,
                        Type = typeof(BannerMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_INFO_RESPONSE",
                        Number = 61,
                        Type = typeof(InformationResponseMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_FAILURE",
                        Number = 51,
                        Type = typeof(FailureMessage)
                    },
                    new MessageMetadata {Name = "SSH_MSG_DEBUG", Number = 4, Type = typeof(DebugMessage),},
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_KEXDH_INIT",
                        Number = 30,
                        Type = typeof(KeyExchangeDhInitMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_GLOBAL_REQUEST",
                        Number = 80,
                        Type = typeof(GlobalRequestMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_OPEN",
                        Number = 90,
                        Type = typeof(ChannelOpenMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_OPEN_CONFIRMATION",
                        Number = 91,
                        Type = typeof(ChannelOpenConfirmationMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_INFO_REQUEST",
                        Number = 60,
                        Type = typeof(InformationRequestMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_UNIMPLEMENTED",
                        Number = 3,
                        Type = typeof(UnimplementedMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_REQUEST_SUCCESS",
                        Number = 81,
                        Type = typeof(RequestSuccessMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_SUCCESS",
                        Number = 99,
                        Type = typeof(ChannelSuccessMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ",
                        Number = 60,
                        Type = typeof(PasswordChangeRequiredMessage)
                    },
                    new MessageMetadata {Name = "SSH_MSG_DISCONNECT", Number = 1, Type = typeof(DisconnectMessage)},
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_SERVICE_REQUEST",
                        Number = 5,
                        Type = typeof(ServiceRequestMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_KEX_DH_GEX_REQUEST",
                        Number = 34,
                        Type = typeof(KeyExchangeDhGroupExchangeRequest)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_KEX_DH_GEX_GROUP",
                        Number = 31,
                        Type = typeof(KeyExchangeDhGroupExchangeGroup)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_SUCCESS",
                        Number = 52,
                        Type = typeof(SuccessMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_USERAUTH_PK_OK",
                        Number = 60,
                        Type = typeof(PublicKeyMessage)
                    },
                    new MessageMetadata {Name = "SSH_MSG_IGNORE", Number = 2, Type = typeof(IgnoreMessage)},
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_WINDOW_ADJUST",
                        Number = 93,
                        Type = typeof(ChannelWindowAdjustMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_EOF",
                        Number = 96,
                        Type = typeof(ChannelEofMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_CHANNEL_CLOSE",
                        Number = 97,
                        Type = typeof(ChannelCloseMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_SERVICE_ACCEPT",
                        Number = 6,
                        Type = typeof(ServiceAcceptMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_KEXDH_REPLY",
                        Number = 31,
                        Type = typeof(KeyExchangeDhReplyMessage)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_KEX_DH_GEX_INIT",
                        Number = 32,
                        Type = typeof(KeyExchangeDhGroupExchangeInit)
                    },
                    new MessageMetadata
                    {
                        Name = "SSH_MSG_KEX_DH_GEX_REPLY",
                        Number = 33,
                        Type = typeof(KeyExchangeDhGroupExchangeReply)
                    }
                };
            }

            /// <summary>
            /// Disables and deactivate all messages.
            /// </summary>
            public void Reset()
            {
                foreach (var messageMetadata in _messagesMetadata)
                {
                    messageMetadata.Activated = messageMetadata.Enabled = false;
                }
            }

            public void EnableActivatedMessages()
            {
                foreach (var messageMetadata in _messagesMetadata)
                {
                    if (messageMetadata.Activated)
                        messageMetadata.Enabled = true;
                }
            }

            public Message Create(byte messageNumber)
            {
                var messageMetadata =
                    (from m in _messagesMetadata where m.Number == messageNumber && m.Enabled && m.Activated select m)
                        .FirstOrDefault();
                if (messageMetadata == null)
                    throw new SshException(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid.",
                        messageNumber));

                return messageMetadata.Type.CreateInstance<Message>();
            }

            public void DisableNonKeyExchangeMessages()
            {
                foreach (var messageMetadata in _messagesMetadata)
                {
                    if (messageMetadata.Activated && messageMetadata.Number > 2 &&
                        (messageMetadata.Number < 20 || messageMetadata.Number > 30))
                    {
                        //Console.WriteLine("Disabling " + messageMetadata.Name + "...");

                        messageMetadata.Enabled = false;
                    }
                }
            }

            public void EnableAndActivateMessage(string messageName)
            {
                lock (_messagesMetadata)
                {
                    var messagesMetadata = _messagesMetadata.Where(m => m.Name == messageName);
                    foreach (var messageMetadata in messagesMetadata)
                        messageMetadata.Enabled = messageMetadata.Activated = true;
                }
            }

            public void DisableAndDeactivateMessage(string messageName)
            {
                lock (_messagesMetadata)
                {
                    var messagesMetadata = _messagesMetadata.Where(m => m.Name == messageName);
                    foreach (var messageMetadata in messagesMetadata)
                        messageMetadata.Enabled = messageMetadata.Activated = false;
                }
            }

            private class MessageMetadata
            {
                public string Name { get; set; }

                public byte Number { get; set; }

                public bool Enabled { get; set; }

                public bool Activated { get; set; }

                public Type Type { get; set; }
            }
        }
    }
}
