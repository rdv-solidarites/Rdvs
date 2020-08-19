using System.Net.Http;

namespace Cd62.Rdvs.Controleur
{
    /// <summary>
    /// 
    /// </summary>
    public interface IControleurRdvs
    {
        /// <summary>
        /// Controler une demande
        /// </summary>
        void ControlerRequest(HttpRequestMessage request, string payload);

        /// <summary>
        /// Cree un rendez-vous
        /// </summary>
        void CreerRendezVous(string message);
    }
}
