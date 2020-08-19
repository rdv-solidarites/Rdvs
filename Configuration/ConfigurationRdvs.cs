using Cd62.Rdvs.Configuration.Elements;
using System.Configuration;

namespace Cd62.Rdvs.Configuration
{
    internal class ConfigurationRdvs : ConfigurationSection, IConfigurationRdvs
    {
        [ConfigurationProperty("CleHash")]
        internal ParamString CleHash => (ParamString)base[nameof(CleHash)];
        IParamString IConfigurationRdvs.CleHash => CleHash;

        [ConfigurationProperty("HeaderSignature")]
        internal ParamString HeaderSignature => (ParamString)base[nameof(HeaderSignature)];
        IParamString IConfigurationRdvs.HeaderSignature => HeaderSignature;

        [ConfigurationProperty("ModelRendezVous")]
        internal ParamString ModelRendezVous => (ParamString)base[nameof(ModelRendezVous)];
        IParamString IConfigurationRdvs.ModelRendezVous => ModelRendezVous;

        [ConfigurationProperty("ModelPlageOuverture")]
        internal ParamString ModelPlageOuverture => (ParamString)base[nameof(ModelPlageOuverture)];
        IParamString IConfigurationRdvs.ModelPlageOuverture => ModelPlageOuverture;

        [ConfigurationProperty("RappelMinutesAvantDebut")]
        internal ParamInteger RappelMinutesAvantDebut => (ParamInteger)base[nameof(RappelMinutesAvantDebut)];
        IParamInteger IConfigurationRdvs.RappelMinutesAvantDebut => RappelMinutesAvantDebut;
    }
}
