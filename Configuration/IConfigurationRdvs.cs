using Cd62.Fwk.Noyau.Configuration.Entites;
using Cd62.Rdvs.Configuration.Elements;

namespace Cd62.Rdvs.Configuration
{
    /// <summary>
    /// Interface de la configuration de Rdvs
    /// </summary>
    [SectionRacine("Rdvs")]
    public interface IConfigurationRdvs : IConfiguration
    {
        /// <summary>
        /// Clé de génération du hash
        /// </summary>
        IParamString CleHash { get; }
        
        /// <summary>
        /// Nom du header contenant la signature
        /// </summary>
        IParamString HeaderSignature { get; }
        
        /// <summary>
        /// Nom du model des rendez-vous
        /// </summary>
        IParamString ModelRendezVous { get; }
        
        /// <summary>
        /// Nom du model des plages d'ouverture
        /// </summary>
        IParamString ModelPlageOuverture { get; }

        /// <summary>
        /// Durée en minutes avant le rappel d'un rendez-vous
        /// Non applicable sur Plage Horaire
        /// </summary>
        IParamInteger RappelMinutesAvantDebut { get; }
    }
}
