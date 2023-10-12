namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class TestInitializer
    {
        [AssemblyInitialize]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "MSTests requires context parameter")]
        public static async Task Initialize(TestContext context)
        {
            await InfrastructureFixture.Instance.InitializeAsync();
        }

        [AssemblyCleanup]
        public static async Task Cleanup()
        {
            await InfrastructureFixture.Instance.DisposeAsync();
        }
    }
}
