using Cd62.Fwk.Fondamentaux.Instrumentation.Service;
using Cd62.Rdvs.Configuration;
using Cd62.Rdvs.Entites;
using Cd62.Rdvs.Services.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace Cd62.Rdvs.Services.Tests
{
    [TestClass]
    public class RdvControllerTest
    {
        private static readonly IConfigurationRdvs ConfigurationRdvs = Fabrique.Get<IConfigurationRdvs>();

        private static readonly string LapinSignature = ConfigurationRdvs.HeaderSignature.Value;
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(ConfigurationRdvs.CleHash.Value);

        private const string Rdv = "{\"data\": {\"id\": 433,\"agent\": {\"id\": 246,\"email\": \"thomas.guillet+ehpad@beta.gouv.fr\",\"first_name\": \"Thomas\",\"last_name\": \"GUILLET\"},\"end_time\": \"12:00:00\",\"first_day\": \"2020-04-23\",\"ical\": \"BEGIN:VCALENDAR\\r\\nVERSION:2.0\\r\\nPRODID:RDV Solidarités\\r\\nCALSCALE:GREGORIAN\\r\\nMETHOD:REQUEST\\r\\nBEGIN:VTIMEZONE\\r\\nTZID:Europe/Paris\\r\\nBEGIN:DAYLIGHT\\r\\nDTSTART:20200329T030000\\r\\nTZOFFSETFROM:+0100\\r\\nTZOFFSETTO:+0200\\r\\nRRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=3\\r\\nTZNAME:CEST\\r\\nEND:DAYLIGHT\\r\\nBEGIN:STANDARD\\r\\nDTSTART:20201025T020000\\r\\nTZOFFSETFROM:+0200\\r\\nTZOFFSETTO:+0100\\r\\nRRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=10\\r\\nTZNAME:CET\\r\\nEND:STANDARD\\r\\nEND:VTIMEZONE\\r\\nBEGIN:VEVENT\\r\\nDTSTAMP:20200428T095620Z\\r\\nUID:plage_ouverture_433@RDV Solidarités\\r\\nDTSTART;TZID=Europe/Paris:20200423T090000\\r\\nDTEND;TZID=Europe/Paris:20200423T120000\\r\\nCLASS:PUBLIC\\r\\nDESCRIPTION:\\r\\nLOCATION:182 Impasse du Maréchal de Lattre de Tassigny 46000 Cahors\\r\\nSUMMARY:RDV Solidarités Accompagnement des familles\\r\\nRRULE:FREQ=WEEKLY;INTERVAL=1;BYDAY=MO,TU,WE,TH,FR\\r\\nATTENDEE:mailto:thomas.guillet+ehpad@beta.gouv.fr\\r\\nEND:VEVENT\\r\\nEND:VCALENDAR\\r\\n\",\"ical_uid\": \"plage_ouverture_433@RDV Solidarités\",\"lieu\": {\"id\": 125,\"address\": \"182 Impasse du Maréchal de Lattre de Tassigny 46000 Cahors\",\"name\": \"Résidence d'Olt\"},\"motifs\": [{\"id\": 285,\"name\": \"Visite d'un proche\"}],\"organisation\": {\"id\": 83,\"departement\": \"46\",\"name\": \"Département du Lot\"},\"rrule\": \"FREQ=WEEKLY;INTERVAL=1;BYDAY=MO,TU,WE,TH,FR;\",\"start_time\": \"09:00:00\",\"title\": \"Accompagnement des familles\"},\"meta\": {\"model\": \"PlageOuverture\",\"event\": \"created\",\"timestamp\": \"2020-04-28 11:56:20 +0200\"}}";

        [TestMethod]
        public void Payload()
        {
            byte[] hash;
            using (HMACSHA256 sha = new HMACSHA256(Key))
            {
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Rdv));
            }

            using (RdvController rc = new RdvController())
            {
                rc.Request = new HttpRequestMessage();
                rc.Configuration = new HttpConfiguration();
                rc.Request.Headers.Add(LapinSignature, BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant());
                rc.Request.Method = HttpMethod.Post;
                rc.Request.Content = new StringContent(Rdv);

                HttpResponseMessage hrm = rc.NewCalendarEvent();
                string s = hrm.Content.ReadAsStringAsync().Result;

                Reponse r = JsonConvert.DeserializeObject<Reponse>(s);

                Assert.IsTrue(r.Signature);
            }
        }

        [TestMethod]
        public void MauvaisMessage()
        {
            const string p = "Snort";

            byte[] hash;
            using (HMACSHA256 sha = new HMACSHA256(Key))
            {
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(p));
            }

            using (RdvController rc = new RdvController())
            {
                rc.Request = new HttpRequestMessage();
                rc.Configuration = new HttpConfiguration();
                rc.Request.Headers.Add(LapinSignature, BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant());
                rc.Request.Method = HttpMethod.Post;
                rc.Request.Content = new StringContent(p);

                HttpResponseMessage hrm = rc.NewCalendarEvent();
                string s = hrm.Content.ReadAsStringAsync().Result;

                Reponse r = JsonConvert.DeserializeObject<Reponse>(s);

                Assert.IsTrue(r.Signature);
                Assert.IsTrue("Unexpected character encountered while parsing value: S. Path '', line 0, position 0.".Equals(r.Erreur, StringComparison.Ordinal));
            }
        }

        [TestMethod]
        public void MauvaisHash()
        {
            byte[] hash;
            using (HMACSHA256 sha = new HMACSHA256(Encoding.UTF8.GetBytes("clé")))
            {
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Rdv));
            }

            using (RdvController rc = new RdvController())
            {
                rc.Request = new HttpRequestMessage();
                rc.Configuration = new HttpConfiguration();
                rc.Request.Headers.Add(LapinSignature, BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant());
                rc.Request.Method = HttpMethod.Post;
                rc.Request.Content = new StringContent(Rdv);

                HttpResponseMessage hrm = rc.NewCalendarEvent();
                string s = hrm.Content.ReadAsStringAsync().Result;

                Reponse r = JsonConvert.DeserializeObject<Reponse>(s);

                Assert.IsFalse(r.Signature);
            }
        }


        [TestMethod]
        public void NoHash()
        {
            using (RdvController rc = new RdvController())
            {
                rc.Request = new HttpRequestMessage();
                rc.Configuration = new HttpConfiguration();
                rc.Request.Method = HttpMethod.Post;
                rc.Request.Content = new StringContent(Rdv);

                HttpResponseMessage hrm = rc.NewCalendarEvent();
                string s = hrm.Content.ReadAsStringAsync().Result;

                Reponse r = JsonConvert.DeserializeObject<Reponse>(s);

                Assert.IsFalse(r.Signature);
            }
        }
    }
}
