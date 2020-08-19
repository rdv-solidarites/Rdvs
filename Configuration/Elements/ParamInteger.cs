using Cd62.Fwk.Noyau.Configuration.Entites;
using System.Configuration;

namespace Cd62.Rdvs.Configuration.Elements
{
    /// <summary>
    /// Configuration integer
    /// </summary>
    public interface IParamInteger : IParam
    {
        /// <summary>
        /// Valeur
        /// </summary>
        int Value { get; }
    }

    internal class ParamInteger : ConfigurationElement, IParamInteger
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public int Value => (int)base["value"];
    }
}
