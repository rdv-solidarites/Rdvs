using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Http;

namespace Cd62.Rdvs.Services
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Nom de fonction prédéfini")]
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
        }
    }
}
