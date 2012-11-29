using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System.Diagnostics;
using System.IO;

namespace Renci.SshNet.Tests.SftpClientTests
{
    [TestClass]
    public class SynchronizeDirectoriesTest
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_SynchronizeDirectories()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();                

                string sourceDir = Path.GetDirectoryName(uploadedFileName);
                string destDir = "/";
                string searchPattern = Path.GetFileName(uploadedFileName);
                var upLoadedFiles = sftp.SynchronizeDirectories(sourceDir, destDir, searchPattern);

                Assert.IsTrue(upLoadedFiles.Count() > 0);

                foreach (var file in upLoadedFiles)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }


        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_BeginSynchronizeDirectories()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();

                string sourceDir = Path.GetDirectoryName(uploadedFileName);
                string destDir = "/";
                string searchPattern = Path.GetFileName(uploadedFileName);
                
                var asyncResult = sftp.BeginSynchronizeDirectories(sourceDir, 
                    destDir, 
                    searchPattern,
                    null,
                    null
                );

                // Wait for the WaitHandle to become signaled.
                asyncResult.AsyncWaitHandle.WaitOne(1000);

                var upLoadedFiles = sftp.EndSynchronizeDirectories(asyncResult);

                Assert.IsTrue(upLoadedFiles.Count() > 0);

                foreach (var file in upLoadedFiles)
                {
                    Debug.WriteLine(file.FullName);
                }

                // Close the wait handle.
                asyncResult.AsyncWaitHandle.Close();

                sftp.Disconnect();
            }
        }
    }
}
