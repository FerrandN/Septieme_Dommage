using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIChallongeClass
{
    public class JSONReaderDBClass
    {
        public string connectionString  { get; set; }
        public string filename { get; set; }
        public JSONReaderDBClass(string fileName)
        {
            this.filename = fileName;
        }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructureDB data = JsonConvert.DeserializeObject<JSONStructureDB>(json);

                this.connectionString = data.connectionString;
            }
        }
    }

    internal sealed class JSONStructureDB
    {
        public string connectionString { get; set; }
    }
}

