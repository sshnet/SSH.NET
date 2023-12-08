using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ObjectIdentifierTest : TestBase
    {
        [TestMethod]
        public void Constructor_IdentifiersIsNull()
        {
            const ulong[] identifiers = null;

            var actualException = Assert.ThrowsException<ArgumentNullException>(() => new ObjectIdentifier(identifiers));

            Assert.AreEqual(typeof(ArgumentNullException), actualException.GetType());
            Assert.IsNull(actualException.InnerException);
            Assert.AreEqual(nameof(identifiers), actualException.ParamName);
        }

        [TestMethod]
        public void Constructor_LengthOfIdentifiersIsLessThanTwo()
        {
            var identifiers = new[] { 5UL };

            var actualException = Assert.ThrowsException<ArgumentException>(() => new ObjectIdentifier(identifiers));

            Assert.AreEqual(typeof(ArgumentException), actualException.GetType());
            Assert.IsNull(actualException.InnerException);
            ArgumentExceptionAssert.MessageEquals("Must contain at least two elements.", actualException);
            Assert.AreEqual(nameof(identifiers), actualException.ParamName);
        }
    }
}
