using Cd62.Core.Logger;
using Cd62.Fwk.Fondamentaux.Instrumentation.Service;
using Cd62.Rdvs.Controleur;
using Cd62.Rdvs.Entites;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Cd62.Rdvs.Services.Controllers
{
    public class RdvController : ApiBaseController
    {
        private static readonly Logger Log = new Logger("Rdvs");

        [HttpPost]
        [Route("NewCalendarEvent")]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Logge toutes les erreurs")]
        public HttpResponseMessage NewCalendarEvent()
        {
            Log.EcrireInfo("[NewCalendarEvent] > Nouvel événement reçu");

            if (Request == null)
            {
                return HttpResponseMessageNoCache(new Reponse
                {
                    Erreur = "Erreur interne",
                    Signature = false
                });
            }

            DumpRequest("NewCalendarEvent");

            string message = Request.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new Reponse()
                {
                    Erreur = "Message vide",
                    Signature = false
                });
            }

            Log.EcrireDebug($"[NewCalendarEvent] > Message reçu : {message}");

            IControleurRdvs cr = Fabrique.Get<IControleurRdvs>();

            try
            {
                cr.ControlerRequest(Request, message);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new Reponse()
                {
                    Erreur = ex.Message,
                    Signature = false
                });
            }

            try
            {
                cr.CreerRendezVous(message);
            }
            catch (Exception ex)
            {
                Log.EcrireErreur($"[NewCalendarEvent] >> Erreur >> {ex.Message}");
                Log.EcrireErreur($"[NewCalendarEvent] >> Erreur >> StackTrace : {ex.StackTrace}");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new Reponse()
                {
                    Erreur = ex.Message,
                    Signature = true
                });
            }

            return HttpResponseMessageNoCache(new Reponse { Signature = true });
        }

        private void DumpRequest(string type)
        {
            Log.EcrireInfo($"[{type}] >> Dump HttpRequest");

            if (Request.Properties.ContainsKey("MS_HttpContext"))
            {
                HttpRequestBase hrb = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request;

                Log.EcrireInfo($"[{type}] >> REMOTE_ADDR : {hrb.ServerVariables["REMOTE_ADDR"]}");
                Log.EcrireInfo($"[{type}] >> HTTP_X_FORWARDED_FOR : {hrb.ServerVariables["HTTP_X_FORWARDED_FOR"]}");
                Log.EcrireInfo($"[{type}] >> Browser : {JsonConvert.SerializeObject(hrb.Browser, Formatting.None)}");
                Log.EcrireInfo($"[{type}] >> ClientCertificate : {JsonConvert.SerializeObject(hrb.ClientCertificate, Formatting.None)}");
                Log.EcrireInfo($"[{type}] >> ContentEncoding : {hrb.ContentEncoding}");
                Log.EcrireInfo($"[{type}] >> IsSecureConnection :{hrb.IsSecureConnection}");
                Log.EcrireInfo($"[{type}] >> QueryString : {JsonConvert.SerializeObject(hrb.QueryString, Formatting.None)}");
            }

            Log.EcrireInfo($"[{type}] >>  Headers :");

            foreach (KeyValuePair<string, IEnumerable<string>> hd in Request.Headers)
            {
                Log.EcrireInfo($"[{type}] >>> {hd.Key} : {string.Join(",", hd.Value)}");
            }
        }
    }
}
