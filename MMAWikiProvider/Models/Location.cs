using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MMAWikiProvider.Models
{
    public class Location
    {
        public string City { get; set; }

        public string Country { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string State { get; set; }

        public Location()
        {

        }

        public Location(Location l)
        {
            City = l.City;
            Country = l.Country;
            State = l.State;
        }

        public Location(string s)
        {
            var splitted = s.Split(",").ToArray();

            if(splitted.Count() == 3)
            {
                City = splitted[0].Trim();
                State = splitted[1].Trim();
                Country = splitted[2].Trim();
            }
            else if(splitted.Count() == 2)
            {
                City = splitted[0].Trim();
                Country = splitted[1].Trim();
            }
            else
            {
                Country = splitted[0];
            }
        }

        public override string ToString()
        {
            if(string.IsNullOrEmpty(City))
            {
                return Country;
            }
            else if(string.IsNullOrEmpty(State))
            {
                return $"{City}, {Country}";
            }
            else
            {
                return $"{City}, {State}, {Country}";
            }
        }

        public override int GetHashCode() => HashCode.Combine(Country, State, City);

        public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();

        public Location Clone() => new Location(this);
    }
}
