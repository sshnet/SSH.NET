
namespace Renci.SshNet.IntegrationTests.TestsFixtures
{
    /// <summary>
    /// The base class for integration tests
    /// </summary>
    public abstract class IntegrationTestBase
    {
        private readonly InfrastructureFixture _infrastructureFixture;

        /// <summary>
        /// The SSH Server host name.
        /// </summary>
        public string? SshServerHostName
        {
            get
            {
                return _infrastructureFixture.SshServerHostName;
            }
        }

        /// <summary>
        /// The SSH Server host name
        /// </summary>
        public ushort SshServerPort
        {
            get
            {
                return _infrastructureFixture.SshServerPort;
            }
        }

        /// <summary>
        /// The admin user that can use SSH Server.
        /// </summary>
        public SshUser AdminUser
        {
            get
            {
                return _infrastructureFixture.AdminUser;
            }
        }

        /// <summary>
        /// The normal user that can use SSH Server.
        /// </summary>
        public SshUser User
        {
            get
            {
                return _infrastructureFixture.User;
            }
        }

        protected IntegrationTestBase()
        {
            _infrastructureFixture = InfrastructureFixture.Instance;
            ShowInfrastructureInformation();
        }

        private void ShowInfrastructureInformation()
        {
            Console.WriteLine($"SSH Server host name: {_infrastructureFixture.SshServerHostName}");
            Console.WriteLine($"SSH Server port: {_infrastructureFixture.SshServerPort}");
        }
    }
}
