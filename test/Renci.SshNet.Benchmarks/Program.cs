using BenchmarkDotNet.Running;

namespace Renci.SshNet.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // Usage examples:
            // 1. Run all benchmarks:
            //     dotnet run -c Release -- --filter *
            // 2. List all benchmarks:
            //     dotnet run -c Release -- --list flat
            // 3. Run a subset of benchmarks based on a filter (of a benchmark method's fully-qualified name,
            //    e.g. "Renci.SshNet.Benchmarks.Security.Cryptography.Ciphers.AesCipherBenchmarks.Encrypt_CBC"):
            //     dotnet run -c Release -- --filter *Ciphers*
            // 4. Run benchmarks and include memory usage statistics in the output:
            //     dotnet run -c Release -- filter *Rsa* --memory
            // 3. Print help:
            //     dotnet run -c Release -- --help

            // See also https://benchmarkdotnet.org/articles/guides/console-args.html

            _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
