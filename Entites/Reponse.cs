namespace Cd62.Rdvs.Entites
{
    public class Reponse
    {
        public Reponse()
        {
            Erreur = string.Empty;
        }

        public bool Signature { get; set; }
        public string Erreur { get; set; }
    }
}
