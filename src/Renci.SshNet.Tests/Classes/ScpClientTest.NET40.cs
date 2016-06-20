using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClientTest
    {
        [TestMethod]
        [TestCategory("Scp")]
        [TestCategory("integration")]
        public void Test_Scp_File_20_Parallel_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                var uploadFilenames = new string[20];
                for (int i = 0; i < uploadFilenames.Length; i++)
                {
                    uploadFilenames[i] = Path.GetTempFileName();
                    this.CreateTestFile(uploadFilenames[i], 1);
                }

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Upload(new FileInfo(filename), Path.GetFileName(filename));
                    });

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Download(Path.GetFileName(filename), new FileInfo(string.Format("{0}.down", filename)));
                    });

                var result = from file in uploadFilenames
                             where
                                 CalculateMD5(file) == CalculateMD5(string.Format("{0}.down", file))
                             select file;

                scp.Disconnect();

                Assert.IsTrue(result.Count() == uploadFilenames.Length);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        [TestCategory("integration")]
        public void Test_Scp_File_Upload_Download_Events()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                var uploadFilenames = new string[10];

                for (int i = 0; i < uploadFilenames.Length; i++)
                {
                    uploadFilenames[i] = Path.GetTempFileName();
                    this.CreateTestFile(uploadFilenames[i], 1);
                }

                var uploadedFiles = uploadFilenames.ToDictionary((filename) => Path.GetFileName(filename), (filename) => 0L);
                var downloadedFiles = uploadFilenames.ToDictionary((filename) => string.Format("{0}.down", Path.GetFileName(filename)), (filename) => 0L);

                scp.Uploading += delegate(object sender, ScpUploadEventArgs e)
                {
                    uploadedFiles[e.Filename] = e.Uploaded;
                };

                scp.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                {
                    downloadedFiles[string.Format("{0}.down", e.Filename)] = e.Downloaded;
                };

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Upload(new FileInfo(filename), Path.GetFileName(filename));
                    });

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Download(Path.GetFileName(filename), new FileInfo(string.Format("{0}.down", filename)));
                    });

                var result = from uf in uploadedFiles
                             from df in downloadedFiles
                             where
                                 string.Format("{0}.down", uf.Key) == df.Key
                                 && uf.Value == df.Value
                             select uf;

                scp.Disconnect();

                Assert.IsTrue(result.Count() == uploadFilenames.Length && uploadFilenames.Length == uploadedFiles.Count && uploadedFiles.Count == downloadedFiles.Count);
            }
        }
    }
}