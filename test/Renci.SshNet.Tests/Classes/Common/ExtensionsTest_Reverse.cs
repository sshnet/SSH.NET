﻿using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ExtensionsTest_Reverse
    {
        [TestMethod]
        public void Empty()
        {
            var value = new byte[0];

            var actual = Extensions.Reverse(value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void Null()
        {
            const byte[] value = null;

            try
            {
                Extensions.Reverse(value);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("array", ex.ParamName);
            }
        }

        [TestMethod]
        public void Small()
        {
            var value = new[] { 0, 1, 4, 3, 7, 9 };

            var actual = Extensions.Reverse(value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(6, actual.Length);
            Assert.AreEqual(9, actual[0]);
            Assert.AreEqual(7, actual[1]);
            Assert.AreEqual(3, actual[2]);
            Assert.AreEqual(4, actual[3]);
            Assert.AreEqual(1, actual[4]);
            Assert.AreEqual(0, actual[5]);

            Assert.AreEqual(9, value[0]);
            Assert.AreEqual(7, value[1]);
            Assert.AreEqual(3, value[2]);
            Assert.AreEqual(4, value[3]);
            Assert.AreEqual(1, value[4]);
            Assert.AreEqual(0, value[5]);
        }
    }
}

