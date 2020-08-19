using Cd62.Fwk.Noyau.Configuration.Entites;
using System.Configuration;

namespace Cd62.Rdvs.Configuration.Elements
{
    /// <summary>
    /// Configuration string
    /// </summary>
    public interface IParamString : IParam
    {
        /// <summary>
        /// Valeur
        /// </summary>
        string Value { get; }
    }

    internal class ParamString : ConfigurationElement, IParamString
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value => (string)base["value"];
    }
}
