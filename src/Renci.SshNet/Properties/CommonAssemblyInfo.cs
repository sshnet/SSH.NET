using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyDescription("SSH.NET is a Secure Shell (SSH) library for .NET, optimized for parallelism.")]
[assembly: AssemblyCompany("Renci")]
[assembly: AssemblyProduct("SSH.NET")]
[assembly: AssemblyCopyright("Copyright � Renci 2010-2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("2016.0.0")]
[assembly: AssemblyFileVersion("2016.0.0")]
[assembly: AssemblyInformationalVersion("2016.0.0")]
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]


#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif