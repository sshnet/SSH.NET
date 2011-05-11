using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Renci.SshClient.Tests.Security
{
    [TestClass]
    public class TestPrivateKeyFile
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_EmptyFileName()
        {
            string fileName = string.Empty;
            var keyFile = new PrivateKeyFile(fileName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_StreamIsNull()
        {
            Stream stream = null;
            var keyFile = new PrivateKeyFile(stream);
        }

    }
}
