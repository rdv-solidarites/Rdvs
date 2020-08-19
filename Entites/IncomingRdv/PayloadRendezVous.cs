using Newtonsoft.Json;

namespace Cd62.Rdvs.Entites.IncomingRdv
{
    public class PayloadRendezVous : IPayload
    {
        [JsonProperty("data")]
        public Datas Data { get; set; }

        public Meta Meta { get; set; }
    }
}
