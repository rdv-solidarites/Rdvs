using Newtonsoft.Json;

namespace Cd62.Rdvs.Entites.IncomingPlageOuverture
{
    public class Agent
    {
        public int Id { get; set; }
        public string Email { get; set; }
        
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
    }
}
