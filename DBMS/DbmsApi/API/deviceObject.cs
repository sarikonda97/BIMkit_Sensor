using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbmsApi.API
{
    public class deviceObject
    {
        public string id;
        public string Name;
        public string TypeId;
        public string generalLocation;
        public string deviceId;

        public Properties Properties;
        public List<KeyValuePair<string, string>> Tags = new List<KeyValuePair<string, string>>();
    }
}
