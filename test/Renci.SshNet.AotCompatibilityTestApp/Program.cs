using System;

namespace Renci.SshNet.AotCompatibilityTestApp
{
    public static class Program
    {
        public static void Main()
        {
            // This app is used to verify the trim- and AOT-friendliness of
            // the library and its dependencies, by specifying <TrimmerRootAssembly>
            // in the csproj and publishing with e.g. "dotnet publish -c Release -r win-x64"

            // See https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming?pivots=dotnet-8-0
            // and https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/

            Console.WriteLine("Hello, AOT!");
        }
    }
}
