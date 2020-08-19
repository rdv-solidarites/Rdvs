using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cd62.Rdvs.Entites.IncomingPlageOuverture
{
    public class Datas
    {
        public Datas()
        {
            Motifs = new List<Item>();
        }

        public int Id { get; set; }
        public Agent Agent { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("first_day")]
        public string FirstDay { get; set; }
        
        public string Ical { get; set; }
        
        [JsonProperty("ical_uid")]
        public string IcalUid { get; set; }
        
        public Lieu Lieu { get; set; }
        public List<Item> Motifs { get; }
        public Organisation Organisation { get; set; }
        public string Rrule { get; set; }
        
        [JsonProperty("start_time")]
        public string StartTime { get; set; }
        
        public string Title { get; set; }
    }
}
