using Cd62.Core.AssemblyResolver;
using Cd62.Core.Logger;
using System.IO;

namespace Cd62.Rdvs.Services
{
    public static class AppInitializer
    {
        public static void Initialize()
        {
            if (!Logger.IsConfigured())
            {
                throw new FileLoadException("log4net.config non trouvé.");
            }

            AssemblyResolver.Init();
        }
    }
}