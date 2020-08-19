using Cd62.Core.AssemblyResolver;
using Cd62.Core.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Cd62.Rdvs.Services.Tests
{
    [TestClass]
    public static class Init
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Needed")]
        [SuppressMessage("Style", "CA1801:Remove unused parameter", Justification = "Needed")]
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            if (!Logger.IsConfigured())
            {
                throw new FileLoadException("log4net.config non trouvé.");
            }
            string drive = AppDomain.CurrentDomain.BaseDirectory.Substring(0, 1);
            AssemblyResolver.AddRepertoireAssemblies($"{drive}:\\tfs\\Commun.Net\\FWK\\V6.0\\Technique\\Mail");
            AssemblyResolver.Init();
        }
    }
}
