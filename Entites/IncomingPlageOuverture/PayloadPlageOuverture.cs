using Newtonsoft.Json;

namespace Cd62.Rdvs.Entites.IncomingPlageOuverture
{
    public class PayloadPlageOuverture : IPayload
    {
        [JsonProperty("data")]
        public Datas Data { get; set; }

        public Meta Meta { get; set; }
    }
}
