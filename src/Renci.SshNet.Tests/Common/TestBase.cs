﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace Renci.SshNet.Tests.Common
{
    [TestClass]
    public abstract class TestBase
    {
        private static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        [TestInitialize]
        public void Init()
        {
            OnInit();
        }

        [TestCleanup]
        public void Cleanup()
        {
            OnCleanup();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnCleanup()
        {
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

        protected static Stream GetData(string name)
        {
            return ExecutingAssembly.GetManifestResourceStream(string.Format("Renci.SshNet.Tests.Data.{0}", name));
        }
    }
}
