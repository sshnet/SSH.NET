using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    public abstract class TripleATestBase
    {
        [TestInitialize]
        public void Init()
        {
            Arrange();
            Act();
            AfterAct();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TearDown();
        }

        protected virtual void TearDown()
        {
        }

        protected abstract void Arrange();

        protected abstract void Act();

        protected virtual void AfterAct()
        {
        }
    }
}

