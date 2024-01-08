using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIChallongeClass
{
    public class JSONReaderSubdomainClass
    {

        public string subdomain { get; set; }

        public string filename { get; set; }
        public JSONReaderSubdomainClass(string fileName)
        {
            this.filename = fileName;
        }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.subdomain = data.subdomain;
            }
        }

        internal sealed class JSONStructure
        {
            public string subdomain { get; set; }
        }

    }
}

