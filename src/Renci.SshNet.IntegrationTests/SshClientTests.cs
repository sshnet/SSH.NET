using FluentAssertions;

using System.Text;
using Xunit.Abstractions;

namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SSH client integration tests
    /// </summary>
    [Collection("Infrastructure collection")]
    public class SshClientTests : IntegrationTestBase, IDisposable
    {
        private readonly SshClient _sshClient;

        /// <summary>
        /// The 
        /// </summary>
        /// <param name="testOutputHelper"></param>
        /// <param name="infrastructureFixture"></param>
        public SshClientTests(ITestOutputHelper testOutputHelper, InfrastructureFixture infrastructureFixture)
            : base(testOutputHelper, infrastructureFixture)
        {
            _sshClient = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _sshClient.Connect();
        }

        [Fact]
        public void Test_SSH_Echo_Command()
        {
            var builder = new StringBuilder();
            var response = _sshClient.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'");
            response.Result.Should().Be("test !@#$%^&*()_+{}:,./<>[];\\|\n");

        }
    
        public void Dispose()
        {
            _sshClient.Disconnect();
            _sshClient.Dispose();
        }
    }
}
