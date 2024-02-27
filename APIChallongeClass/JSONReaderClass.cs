using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIChallongeClass
{
    public class JSONReaderClass
    {
        public string token { get; set; }
        public string prefix { get; set; }

        public string filename { get; set; }
        public JSONReaderClass(string fileName) 
        {
            this.filename = fileName;
        }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.token = data.token;
                if(data.prefix != null)
                {
                    prefix = data.prefix;
                }
            }
        }
    }

    internal sealed class JSONStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
