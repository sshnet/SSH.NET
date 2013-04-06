using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SshNet")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("Renci")]
[assembly: AssemblyProduct("SSH.NET")]
[assembly: AssemblyCopyright("Copyright © Renci 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ad816c5e-6f13-4589-9f3e-59523f8b77a4")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2013.4.7")]
[assembly: AssemblyFileVersion("2013.4.7")]
[assembly: AssemblyInformationalVersion("2013.4.7")]
[assembly: CLSCompliant(false)]
[assembly: InternalsVisibleTo("Renci.SshNet.Tests")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif