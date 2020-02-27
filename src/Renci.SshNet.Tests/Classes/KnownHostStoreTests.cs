using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class KnownHostStoreTests
    {
        private static readonly byte[] ExampleKey = new byte[]
        {
            0x00, 0x11, 0x22, 0x33, 0x44,
            0x55, 0x66, 0x77, 0x88, 0x99,
            0xaa, 0xbb, 0xcc, 0xdd, 0xee,
            0xff
        };

        private const string ExampleHost = "some.domain.com";
        private const string ExampleSubDomain = "*.domain.com";
        private const string ExampleKeyType = "rsa";

        [TestMethod]
        public void TestKnownHostStoreFindsHostIfMatch()
        {
            KnownHostStore hostStore = new KnownHostStore();
            hostStore.AddHost(ExampleHost, 22, ExampleKeyType, ExampleKey, false);
            Assert.IsTrue(hostStore.Knows(ExampleHost, ExampleKeyType, ExampleKey, 22));
        }

        [TestMethod]
        public void TestKnownHostStoreFindsHashedHostIfMatch()
        {
            KnownHostStore hostStore = new KnownHostStore();
            hostStore.AddHost(ExampleHost, 22, ExampleKeyType, ExampleKey, true);
            Assert.IsTrue(hostStore.Knows(ExampleHost, ExampleKeyType, ExampleKey, 22));
        }

        [TestMethod]
        [ExpectedException(typeof(RevokedKeyException), "Did not throw an exception upon finding a revoked key in the store")]
        public void TestKnownHostStoreRejectsKeyIfRevoked()
        {
            KnownHostStore hostStore = new KnownHostStore();
            hostStore.AddHost(ExampleHost, 22, ExampleKeyType, ExampleKey, false);
            hostStore.AddHost(ExampleSubDomain, 22, ExampleKeyType, ExampleKey, false, "@revoked");

            hostStore.Knows(ExampleHost, ExampleKeyType, ExampleKey, 22);
        }
    }
}
