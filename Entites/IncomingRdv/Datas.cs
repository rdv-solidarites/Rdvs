using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cd62.Rdvs.Entites.IncomingRdv
{
    public class Datas
    {
        public Datas()
        {
            Agents = new List<AgentRendezVous>();
            Users = new List<User>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; }
        public string Address { get; set; }

        [JsonProperty("duration_in_min")]
        public string DurationInMin { get; set; }

        [JsonProperty("agents")]
        public List<AgentRendezVous> Agents { get; }
        
        public Motif Motif { get; set; }
        public Organisation Organisation { get; set; }
        
        [JsonProperty("starts_at")]
        public string StartsAt { get; set; }
        
        public string Status { get; set; }
        public List<User> Users { get; }
    }
}
