using System.Text;
using FluentAssertions;
using Renci.SshNet.Common;
using Xunit.Abstractions;

namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SFTP client integration tests
    /// </summary>
    [Collection("Infrastructure collection")]
    public class SftpClientTests : IntegrationTestBase, IDisposable
    {
        private readonly SftpClient _sftpClient;

        public SftpClientTests(ITestOutputHelper testOutputHelper, InfrastructureFixture infrastructureFixture)
            : base(testOutputHelper, infrastructureFixture)
        {
            _sftpClient = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _sftpClient.Connect();
        }

        [Fact]
        public void Test_Sftp_ListDirectory_Home_Directory()
        {
            var builder = new StringBuilder();
            var files = _sftpClient.ListDirectory("/");
            foreach (var file in files)
            {
                builder.AppendLine($"{file.FullName}");
            }

            builder.ToString().Should().Be(@"/usr
/var
/.
/bin
/mnt
/opt
/tmp
/etc
/root
/media
/..
/dev
/proc
/sys
/home
/lib
/sbin
/run
/srv
/.dockerenv
");
        }

        [Fact]
        public void Test_Sftp_ListDirectory_Permission_Denied()
        {
            Action act = () => _sftpClient.ListDirectory("/root");

            act.Should().Throw<SshException>().WithMessage("Permission denied");

        }

        public void Dispose()
        {
            _sftpClient.Disconnect();
            _sftpClient.Dispose();
        }
    }
}
