using Cd62.Rdvs.Services;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;

[assembly: AssemblyTitle("Rdvs.Services")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Rdvs")]
[assembly: AssemblyCopyright("Copyright © 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("56f56475-db63-4441-90a6-6c7a81e7a40d")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyKeyFile("..\\..\\..\\Rdvs.snk")]
[assembly: PreApplicationStartMethod(typeof(AppInitializer), "Initialize")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: Debuggable(true, true)]
#else
[assembly: AssemblyConfiguration("Release")]
[assembly: Debuggable(false, false)]
#endif