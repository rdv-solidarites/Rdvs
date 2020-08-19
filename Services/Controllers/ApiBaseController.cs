using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Cd62.Rdvs.Services.Controllers
{
    public class ApiBaseController : ApiController
    {
        public HttpResponseMessage HttpResponseMessageCache(object objet)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, objet);
            response.Content.Headers.ContentEncoding.Add("UTF-8");
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = new TimeSpan(0, 1, 0, 0),
                NoStore = false,
                NoCache = false
            };
            return response;
        }

        public HttpResponseMessage HttpResponseMessageNoCache(object objet)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, objet);
            response.Content.Headers.ContentEncoding.Add("UTF-8");
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = false,
                NoStore = true,
                NoCache = true,
                MaxAge = new TimeSpan(0, 0, 0, 0)
            };
            return response;
        }
    }
}