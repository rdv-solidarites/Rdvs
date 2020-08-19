using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;

namespace Cd62.Rdvs.Entites.IncomingRdv
{
    public class User
    {
        public int Id { get; set; }
        public string Address { get; set; }

        [JsonProperty("birth_date")]
        public string BirthDate { get; set; }

        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        public User Responsible { get; set; }

        public string ToHtml()
        {
            string bd = BirthDate;
            if (!string.IsNullOrEmpty(bd))
            {
                bd = Convert.ToDateTime(bd, CultureInfo.InvariantCulture).ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
            }
            
            StringBuilder sb = new StringBuilder();
            sb.Append($"<li>Nom : {FirstName} {LastName}</li>");
            //sb.Append($"<li>Date de naissance : {(string.IsNullOrEmpty(bd) ? "Inconnue" : bd)}</li>");
            sb.Append($"<li>Adresse : {(string.IsNullOrEmpty(Address) ? "Inconnue" : Address)}</li>");

            if (!string.IsNullOrEmpty(Email))
            {
                sb.Append($"<li>Email : {Email}</li>");
            }
            
            if (Responsible != null)
            {
                sb.Append("<li>Responsable:");
                sb.Append($"<ul>{Responsible.ToHtml()}</ul>");
                sb.Append("</li>");
            }

            return sb.ToString();
        }
    }
}
