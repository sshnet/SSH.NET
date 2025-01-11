using BenchmarkDotNet.Attributes;

using Renci.SshNet.Common;

namespace Renci.SshNet.Benchmarks.Common
{
    public class ExtensionsBenchmarks
    {
        private byte[]? _data;

        [Params(1000, 10000)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _data = new byte[N];
            new Random(42).NextBytes(_data);
        }

        [Benchmark]
        public byte[] Reverse()
        {
            return _data.Reverse();
        }
    }
}
