using System.Formats.Asn1;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using APIChallongeClass;

namespace challongeAPI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using HttpClient client = new();
            var jsonReader = new JSONReaderClass("jsconfig1.json");
            await jsonReader.ReadJSON();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonReader.token)));

            //await ProcessRepositoriesAsync(client)
            await Test(client);

/*            static async Task ProcessRepositoriesAsync(HttpClient client)
            {  
                var json = await client.GetStringAsync(
                "https://api.challonge.com/v1/");

                Console.Write(json);
            }*/

            static async Task Test(HttpClient client)
            {
                const string URL = "https://api.challonge.com/v1/";
                const string querry = URL + "tournaments.json";

                var response = await client.GetAsync(querry);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
