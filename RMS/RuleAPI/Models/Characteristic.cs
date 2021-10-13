using DbmsApi;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DbmsApi.API;

namespace RuleAPI.Models
{
    public class Characteristic
    {
        //[JsonConverter(typeof(StringEnumConverter))]
        public string Type { get; set; }

        public List<PropertyCheck> PropertyChecks { get; set; }

        public Characteristic(string type, List<PropertyCheck> propertyChecks)
        {
            Type = type;
            PropertyChecks = propertyChecks;
        }

        public string String()
        {
            string returnString = Type.ToString();
            List<string> pcList = new List<string>();
            foreach (PropertyCheck pc in PropertyChecks)
            {
                pcList.Add(pc.String());
            }
            returnString += pcList.Count > 0 ? " with {" : "";
            returnString += string.Join(" AND ", pcList);
            returnString += pcList.Count > 0 ? "}" : "";
            return returnString;
        }

        public Characteristic Copy()
        {
            List<PropertyCheck> newPropertyChecks = new List<PropertyCheck>();
            foreach (PropertyCheck ec in PropertyChecks)
            {
                newPropertyChecks.Add(ec.Copy());
            }

            return new Characteristic(this.Type, newPropertyChecks);
        }
    }
}