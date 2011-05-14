using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Renci.SshNet.Tests.Security
{
    [TestClass]
    public class TestPrivateKeyFile
    {
		[WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_EmptyFileName()
        {
            string fileName = string.Empty;
            var keyFile = new PrivateKeyFile(fileName);
        }

		[WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_StreamIsNull()
        {
            Stream stream = null;
            var keyFile = new PrivateKeyFile(stream);
        }

    }
}
