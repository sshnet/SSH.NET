using Xunit.Abstractions;

namespace Renci.SshNet.IntegrationTests.TestsFixtures
{
    /// <summary>
    /// The base class for integration tests
    /// </summary>
    public abstract class IntegrationTestBase
    {
        private readonly InfrastructureFixture _infrastructureFixture;

        /// <summary>
        /// Represents a class which can be used to provide test output.
        /// </summary>
        public ITestOutputHelper TestOutputHelper { get; }

        /// <summary>
        /// The SSH Server host name.
        /// </summary>
        public string? SshServerHostName => _infrastructureFixture.SshServerHostName;

        /// <summary>
        /// The SSH Server host name
        /// </summary>
        public ushort SshServerPort => _infrastructureFixture.SshServerPort;

        /// <summary>
        /// The admin user that can use SSH Server.
        /// </summary>
        public SshUser AdminUser => _infrastructureFixture.AdminUser;

        /// <summary>
        /// The normal user that can use SSH Server.
        /// </summary>
        public SshUser User => _infrastructureFixture.User;


        protected IntegrationTestBase(ITestOutputHelper testOutputHelper, InfrastructureFixture infrastructureFixture)
        {
            _infrastructureFixture = infrastructureFixture;
            TestOutputHelper = testOutputHelper;
            ShowInfrastructureInformation();
        }

        private void ShowInfrastructureInformation()
        {
            TestOutputHelper.WriteLine($"SSH Server host name: {_infrastructureFixture.SshServerHostName}");
            TestOutputHelper.WriteLine($"SSH Server port: {_infrastructureFixture.SshServerPort}");
        }
    }
}
