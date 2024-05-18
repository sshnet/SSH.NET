﻿using BenchmarkDotNet.Attributes;

using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Renci.SshNet.Benchmarks.Common
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class HostKeyEventArgsBenchmarks
    {
        private readonly KeyHostAlgorithm _keyHostAlgorithm;

        public HostKeyEventArgsBenchmarks()
        {
            _keyHostAlgorithm = GetKeyHostAlgorithm();
        }
        private static KeyHostAlgorithm GetKeyHostAlgorithm()
        {
            using (var s = typeof(HostKeyEventArgsBenchmarks).Assembly.GetManifestResourceStream("Renci.SshNet.Benchmarks.Data.Key.RSA.txt"))
            {
                var privateKey = new PrivateKeyFile(s);
                return (KeyHostAlgorithm)privateKey.HostKeyAlgorithms.First();
            }
        }

        [Benchmark()]
        public HostKeyEventArgs Constructor()
        {
            return new HostKeyEventArgs(_keyHostAlgorithm);
        }

        [Benchmark()]
        public (string, string) CalculateFingerPrintSHA256AndMD5()
        {
            var test = new HostKeyEventArgs(_keyHostAlgorithm);

            return (test.FingerPrintSHA256, test.FingerPrintMD5);
        }

        [Benchmark()]
        public string CalculateFingerPrintSHA256()
        {
            var test = new HostKeyEventArgs(_keyHostAlgorithm);

            return test.FingerPrintSHA256;
        }

        [Benchmark()]
        public byte[] CalculateFingerPrint()
        {
            var test = new HostKeyEventArgs(_keyHostAlgorithm);

            return test.FingerPrint;
        }

        [Benchmark()]
        public string CalculateFingerPrintMD5()
        {
            var test = new HostKeyEventArgs(_keyHostAlgorithm);

            return test.FingerPrintSHA256;
        }
    }
}
