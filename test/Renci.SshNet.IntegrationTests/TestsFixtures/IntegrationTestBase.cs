using System.Diagnostics;

using Renci.SshNet.Abstractions;

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
        public string SshServerHostName
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

        /// <summary>
        /// Creates the test file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Size in megabytes.</param>
        protected void CreateTestFile(string fileName, int size)
        {
            using (var testFile = File.Create(fileName))
            {
                var random = new Random();
                for (int i = 0; i < 1024 * size; i++)
                {
                    var buffer = new byte[1024];
                    random.NextBytes(buffer);
                    testFile.Write(buffer, 0, buffer.Length);
                }
            }
        }

        protected void EnableTracing()
        {
            DiagnosticAbstraction.Source.Switch = new SourceSwitch("sourceSwitch", nameof(TraceEventType.Verbose));
            DiagnosticAbstraction.Source.Listeners.Remove("Default");
            DiagnosticAbstraction.Source.Listeners.Add(new ConsoleTraceListener() { Name = "TestConsoleLogger" });
        }

        protected void DisableTracing()
        {
            DiagnosticAbstraction.Source.Switch = null;
            DiagnosticAbstraction.Source.Listeners.Remove("TestConsoleLogger");
        }
    }
}
