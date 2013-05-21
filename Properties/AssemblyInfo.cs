using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Inedo.BuildMaster.Extensibility;

[assembly: AssemblyTitle("Azure")]
[assembly: AssemblyDescription("Contains actions to interface with Windows Azure Services.")]

[assembly: ComVisible(false)]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyProduct("BuildMaster")]
[assembly: AssemblyCopyright("Copyright © 2008 - 2013")]
[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0")]
[assembly: BuildMasterAssembly]
[assembly: CLSCompliant(false)]
[assembly: RequiredBuildMasterVersion("3.5.0")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Azure.Tests")]