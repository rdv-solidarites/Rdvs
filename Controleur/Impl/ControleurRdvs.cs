using Cd62.Core.Logger;
using Cd62.Fwk.Fondamentaux.Instrumentation.Service;
using Cd62.Fwk.Noyau.Outils;
using Cd62.Fwk.Technique.Mail.Entites;
using Cd62.Fwk.Technique.Mail.Entites.CriteresRecherche;
using Cd62.Fwk.Technique.Mail.Service;
using Cd62.Rdvs.Configuration;
using Cd62.Rdvs.Entites;
using Cd62.Rdvs.Entites.IncomingPlageOuverture;
using Cd62.Rdvs.Entites.IncomingRdv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;

namespace Cd62.Rdvs.Controleur.Impl
{
    internal class ControleurRdvs : IControleurRdvs
    {
        private static readonly Logger Log = new Logger("Rdvs");
        private static readonly IConfigurationRdvs ConfigurationRdvs = Fabrique.Get<IConfigurationRdvs>();
        private const int NombreMaximumResultats = 500;

        private static readonly string LapinSignature = ConfigurationRdvs.HeaderSignature.Value;
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(ConfigurationRdvs.CleHash.Value);

        private static readonly string TypeRendezVous = ConfigurationRdvs.ModelRendezVous.Value;
        private static readonly string TypePlageOuverture = ConfigurationRdvs.ModelPlageOuverture.Value;

        private static readonly int RappelMinutesAvantDebut = ConfigurationRdvs.RappelMinutesAvantDebut.Value;

        private const string SanitizeSujet = "&^$#@!(),<>*";
        private const string SanitizeLieu = "?&^$#@!()+-,:;<>’_*";
        private const string SanitizeNom = "?&^$#@!()+-,:;<>’_*";

        private const string Create = "created";
        private const string Update = "updated";
        private const string Delete = "destroyed";

        private const string StatusExcused = "excused";
        private const string StatusNotExcused = "notexcused";

        public void ControlerRequest(HttpRequestMessage request, string payload)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.Headers.Contains(LapinSignature))
            {
                Log.EcrireErreur("[ControleurRdv] > Signature manquante");
                throw new InvalidDataException("Signature manquante");
            }

            if (!VerifierSignature(payload, request.Headers.GetValues(LapinSignature).First()))
            {
                Log.EcrireErreur("[ControleurRdv] > Signature invalide");
                throw new InvalidDataException("Signature invalide");
            }
        }

        public void CreerRendezVous(string message)
        {
            // La balise Meta est commune
            // On désérialise sur un payload quelconque pour la récupérer
            Meta meta = JsonConvert.DeserializeObject<PayloadRendezVous>(message).Meta;

            IPayload payload;
            IRendezVous rendezVous;

            if (TypeRendezVous.Equals(meta.Model, StringComparison.Ordinal))
            {
                payload = DeserialiserRendezVous(message);
                rendezVous = InitialiserRendezVous((PayloadRendezVous)payload);
            }
            else if (TypePlageOuverture.Equals(meta.Model, StringComparison.Ordinal))
            {
                payload = DeserialiserPlageOuverture(message);
                rendezVous = InitialiserPlageOuverture((PayloadPlageOuverture)payload);
            }
            else
            {
                throw new NotSupportedException("Model non supporté.");
            }

            switch (meta.Event)
            {
                case Create:
                    Log.EcrireInfo("[NewCalendarEvent] >> Nouveau rdv");
                    LogEvent(rendezVous);

                    Fabrique.Get<IServiceExchange>().CreerRendezVous(rendezVous);
                    break;
                case Update:
                    Log.EcrireInfo("[NewCalendarEvent] >> mise à jour rdv");
                    LogEvent(rendezVous);

                    IList<IRendezVous> lrvm = ChercherRendezVousDansLeMois(rendezVous);

                    if (!lrvm.Any())
                    {
                        throw new InvalidDataException("Aucune plage trouvée dans le mois spécifié.");
                    }

                    rendezVous.Id = lrvm[0].Id; // La recherche donne au moins 1 rdv de la série que je réutilise avec les nouvelles données

                    Fabrique.Get<IServiceExchange>().ModifierRendezVous(rendezVous, true);
                    
                    break;
                case Delete:
                    Log.EcrireInfo("[NewCalendarEvent] >> Suppression rdv");
                    LogEvent(rendezVous);

                    IList<IRendezVous> lrvs = ChercherRendezVousDansLeMois(rendezVous);

                    if (!lrvs.Any())
                    {
                        throw new InvalidDataException("Aucune plage trouvée dans le mois spécifié.");
                    }

                    Fabrique.Get<IServiceExchange>().SupprimerRendezVous(lrvs[0], true);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static IList<IRendezVous> ChercherRendezVousDansLeMois(IRendezVous rendezVous)
        {
            ICritereRechercheRendezVousParExternalId crrv = Fabrique.Get<ICritereRechercheRendezVousParExternalId>();
            crrv.Organisateur = rendezVous.Organisateur;
            crrv.DateDebutRecherche = new DateTime(rendezVous.Debut.Year, rendezVous.Debut.Month, 1);
            crrv.DateFinRecherche = new DateTime(rendezVous.Debut.Year, rendezVous.Debut.Month, 30);
            crrv.NombreMaximumResultats = NombreMaximumResultats;
            crrv.ExternalId = rendezVous.ExternalId;

            return Fabrique.Get<IServiceExchange>().RechercherRendezVous(crrv);
        }

        private static void LogEvent(IRendezVous rendezVous)
        {
            Log.EcrireInfo($"[NewCalendarEvent] >>> Sujet : {rendezVous.Sujet}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> Du : {rendezVous.Debut:F}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> au : {rendezVous.Fin:F}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> Organisateur :{rendezVous.Organisateur.Courriel}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> Lieu : {rendezVous.Lieu}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> Description : {rendezVous.Description}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> ExternalId : {rendezVous.ExternalId}");
            Log.EcrireInfo($"[NewCalendarEvent] >>> Recurrence : {(string.IsNullOrEmpty(rendezVous.Recurrence) ? "Aucune" : rendezVous.Recurrence)}");
        }


        private static IPayload DeserialiserRendezVous(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            PayloadRendezVous payload;
            try
            {
                payload = JsonConvert.DeserializeObject<PayloadRendezVous>(message);
            }
            catch (JsonReaderException jre)
            {
                Log.EcrireErreur($"[ControleurRdv] > Erreur désérialisation : {jre.Message}");
                throw new InvalidDataException("Données non conformes", jre);
            }

            if (!payload.Data.Agents.Any())
            {
                throw new InvalidDataException("Aucun agent n'a été spécifié.");
            }

            if (!Validateur.GetRegExEmail().IsMatch(payload.Data.Agents[0].Email))
            {
                throw new InvalidDataException("L'email n'est pas valide.");
            }

            return payload;
        }

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Email en minuscule")]
        private static IRendezVous InitialiserRendezVous(PayloadRendezVous payload)
        {
            IRendezVous rdv = Fabrique.Get<IRendezVous>();

            switch (payload.Data.Status)
            {
                case StatusExcused:
                    rdv.Sujet = $"EXCUSE - [rdv-solidarités] - {Sanitize(payload.Data.Motif.Name, SanitizeSujet)}";
                    rdv.StatutAffichage = StatutAffichage.Disponible;
                    break;
                case StatusNotExcused:
                    rdv.Sujet = $"NON EXCUSE - [rdv-solidarités] - {Sanitize(payload.Data.Motif.Name, SanitizeSujet)}";
                    rdv.StatutAffichage = StatutAffichage.Disponible;
                    break;
                default:
                    rdv.Sujet = $"[rdv-solidarités] - {Sanitize(payload.Data.Motif.Name, SanitizeSujet)}";
                    rdv.StatutAffichage = StatutAffichage.AbsentduBureau;
                    break;
            }

            rdv.Lieu = Sanitize(payload.Data.Address, SanitizeLieu);

            int duration = int.Parse(payload.Data.DurationInMin, CultureInfo.InvariantCulture);
            rdv.Debut = DateTime.ParseExact(payload.Data.StartsAt, "yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
            rdv.Fin = rdv.Debut.Add(new TimeSpan(0, duration, 0));

            AgentRendezVous organisateur = payload.Data.Agents[0];
            IParticipant po = Fabrique.Get<IParticipant>();
            po.Nom = Sanitize($"{organisateur.LastName} {organisateur.FirstName}", SanitizeNom).Trim();
            po.Courriel = organisateur.Email.ToLowerInvariant();
            rdv.Organisateur = po;

            rdv.RappelMinutesAvantDebut = RappelMinutesAvantDebut;
            rdv.ExternalId = payload.Data.Uuid;

            StringBuilder sb = new StringBuilder();
            if (payload.Data.Users.Count > 1)
            {
                sb.Append("Usagers concernés : <br/>");
            }

            for (int i = 0; i < payload.Data.Users.Count; i++)
            {
                sb.Append($"Usager n°{i + 1}");
                sb.Append("<ul>");
                sb.Append($"{payload.Data.Users[i].ToHtml()}");
                sb.Append("</ul>");
            }

            rdv.Description = sb.ToString();

            return rdv;
        }

        private static IPayload DeserialiserPlageOuverture(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            PayloadPlageOuverture payload;

            try
            {
                payload = JsonConvert.DeserializeObject<PayloadPlageOuverture>(message);
            }
            catch (JsonReaderException jre)
            {
                Log.EcrireErreur($"[ControleurRdv] > Erreur désérialisation : {jre.Message}");
                throw new InvalidDataException("Données non conformes", jre);
            }

            if (payload.Data.Agent == null)
            {
                throw new InvalidDataException("Aucun agent n'a été spécifié.");
            }

            if (!Validateur.GetRegExEmail().IsMatch(payload.Data.Agent.Email))
            {
                throw new InvalidDataException("L'email n'est pas valide.");
            }

            if (payload.Data.Motifs == null || string.IsNullOrEmpty(payload.Data.Motifs[0].Name))
            {
                throw new InvalidDataException("Aucun motif n'a été spécifié.");
            }

            if (payload.Data.Lieu == null || string.IsNullOrEmpty(payload.Data.Lieu.Address))
            {
                throw new InvalidDataException("Aucun lieu n'a été spécifié.");
            }

            return payload;
        }

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Email en minuscule")]
        public static IRendezVous InitialiserPlageOuverture(PayloadPlageOuverture payload)
        {
            IRendezVous rdv = Fabrique.Get<IRendezVous>();
            rdv.Sujet = $"[rdv-solidarités] - {Sanitize($"Plage d'ouverture : {string.Join(" - ", payload.Data.Motifs.Select(i => i.Name))}", SanitizeSujet)}";
            rdv.Lieu = Sanitize(payload.Data.Lieu.Address, SanitizeLieu);

            DateTime dtd = DateTime.ParseExact(payload.Data.FirstDay, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime dts = DateTime.ParseExact(payload.Data.StartTime, "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime dtf = DateTime.ParseExact(payload.Data.EndTime, "HH:mm:ss", CultureInfo.InvariantCulture);

            rdv.Debut = dtd.Add(new TimeSpan(dts.Hour, dts.Minute, dts.Second));
            rdv.Fin = dtd.Add(new TimeSpan(dtf.Hour, dtf.Minute, dtf.Second));

            Agent organisateur = payload.Data.Agent;
            IParticipant po = Fabrique.Get<IParticipant>();
            po.Nom = Sanitize($"{organisateur.LastName} {organisateur.FirstName}", SanitizeNom).Trim();
            po.Courriel = organisateur.Email.ToLowerInvariant();
            rdv.Organisateur = po;

            rdv.Recurrence = payload.Data.Rrule;

            rdv.StatutAffichage = StatutAffichage.Occupe;
            rdv.ExternalId = payload.Data.IcalUid;

            return rdv;
        }

        private static bool VerifierSignature(string message, string providedSignature)
        {
            byte[] providedSignatureBytes = SoapHexBinary.Parse(providedSignature).Value;

            Log.EcrireDebug($"[ControleurRdv] >> Clé transmise : {BitConverter.ToString(providedSignatureBytes).Replace("-", "").ToUpperInvariant()}");

            using (HMACSHA256 sha = new HMACSHA256(Key))
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                Log.EcrireDebug($"[ControleurRdv] >> Clé générée : {BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant()}");
                return providedSignatureBytes.SequenceEqual(hash);
            }
        }

        public static string Sanitize(string dirtyString, string characters)
        {
            if (string.IsNullOrEmpty(dirtyString))
            {
                return string.Empty;
            }

            HashSet<char> removeChars = new HashSet<char>(characters);
            StringBuilder result = new StringBuilder(dirtyString.Length);
            foreach (char c in dirtyString.Where(c => !removeChars.Contains(c)))
            {
                result.Append(c);
            }

            return result.ToString();
        }
    }
}
