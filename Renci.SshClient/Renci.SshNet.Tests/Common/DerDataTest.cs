using Renci.SshNet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Renci.SshNet.Tests.Common
{
    [TestClass()]
    public class DerDataTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        //  TODO:   Restore those tests

        //[TestMethod]
        //[TestCategory("DER")]
        //[Description("Long form, test example given in 8.1.3.5")]
        //[Owner("Kenneth_aa")]
        //[DeploymentItem("Renci.SshNet.dll")]
        //public void Test_Der_GetLength_LongForm_MustNotFail()
        //{
        //    // Taken from example in 8.1.3.5
        //    // L = 201 can be encoded as: 
        //    //         1 0 0 0 0 0 0 1
        //    //         1 1 0 0 1 0 0 1
        //    int length = 201; 
        //    byte[] expected = new byte[2]
        //    {
        //        0x81, // 1 0 0 0 0 0 0 1
        //        0xC9  // 1 1 0 0 1 0 0 1
        //    };
            
        //    byte[] actual = Helper_GetLength(length);
        //    Helper_CompareByteArray(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        //[TestMethod]
        //[TestCategory("DER")]
        //[Description("Short form, test example given in 8.1.3.5")]
        //[Owner("Kenneth_aa")]
        //[DeploymentItem("Renci.SshNet.dll")]
        //public void Test_Der_GetLength_ShortForm_MustNotFail()
        //{
        //    int length = 127;
        //    byte[] expected = new byte[1]
        //    {
        //        0x7F // 0 1 1 1 1 1 1 1
        //    };

        //    byte[] actual = Helper_GetLength(length);
        //    Helper_CompareByteArray(expected, actual);
        //}

        /// <summary>
        /// Compares 2 byte arrays.
        /// </summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="actual">Actual result.</param>
        void Helper_CompareByteArray(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length, "Length mismatch: Expected.Length = {0} - Actual.Length = {1}", expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual<byte>(expected[i], actual[i], "Byte mismatch at index {0}", i);
        }

        ///// <summary>
        ///// Wrapper for calling DerData.GetLength()
        ///// </summary>
        //byte[] Helper_GetLength(int length)
        //{
        //    DerData_Accessor target = new DerData_Accessor();
        //    return target.GetLength(length);
        //}
    }
}
