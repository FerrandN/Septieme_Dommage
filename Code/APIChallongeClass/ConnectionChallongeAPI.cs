using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static APIChallongeClass.Participants;

namespace APIChallongeClass
{
    public static class ConnectionChallongeAPI
    {
        public static async Task<string> GetJson(string id)
        {
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));
            return await GetTournament(client, id);
        }

        public static async Task PostTournament(Dictionary<string, string> dic)
        {
            //set client
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            //set API URL
            const string URL = "https://api.challonge.com/v1/";
            const string querry = URL + "tournaments.json";

            //add token to dictionnary for authentification
            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", jsonToken.token);
            }

            //Post datas to API
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            HttpResponseMessage response = await client.PostAsync(querry,content);
            await response.Content.ReadAsStringAsync();
        }

        public static async Task AddParticipant(Dictionary<string, string> dic)
        {
            //set client
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            //set API URL
            const string URL = "https://api.challonge.com/v1/tournaments/";
            string tournamentId = "";

            //add token to dictionnary for authentification
            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", jsonToken.token);
            }

            //set datas for API
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);
            if(dic.ContainsKey("{tournament}"))
            {
                tournamentId = dic["{tournament}"];
            }

            //Post datas to API
            HttpResponseMessage response = await client.PostAsync(URL + tournamentId + "/participants.json", content);
            await response.Content.ReadAsStringAsync();
        }

        public static async Task DeleteTournament(Dictionary<string, string> dic)
        {
            //set client
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            //connect to client

            //set API URL
            string URL = $"https://{jsonToken.prefix}:{jsonToken.token}@api.challonge.com/v1/tournaments";
            string tournamentId = "";

            //set datas for API
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);
            if (dic.ContainsKey("{tournament}"))
            {
                tournamentId = dic["{tournament}"];
            }

            //Delete URL
            HttpResponseMessage response = await client.DeleteAsync(URL + "/" + tournamentId);
            await response.Content.ReadAsStringAsync();
        }
        public static async Task<string> GetTournament(HttpClient client, string id)
        {
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();

            string URL = $"https://{jsonToken.prefix}:{jsonToken.token}@api.challonge.com/v1/tournaments";
            string querry = "";


            if (id == "None")
            {
                //all tournament no subdomain
                querry = URL + ".json";
            }
            else if(id[0] == '/')
            {
                //one specified tournament
                querry = URL + id;
            }
            else
            {
                //all tournament with subdomain
                querry = $"{URL}.json?subdomain={id}";
            }

            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<string> GetTournamentWithMatches(string link)
        {
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            string URL = $"https://{jsonToken.prefix}:{jsonToken.token}@api.challonge.com/v1/tournaments";
            string querry = URL + link;

            HttpResponseMessage response = await client.GetAsync(querry + "/matches.json");
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetParticipant(string id)
        {
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            string URL = $"https://{jsonToken.prefix}:{jsonToken.token}@api.challonge.com/v1/tournaments";
            string querry = "";

            //one specified tournament
            querry = URL + id + "/participants.json" ;


            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task DeleteParticipant(Dictionary<string, string> dic)
        {
            //set client
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            //connect to client

            //set API URL
            string URL = $"https://api.challonge.com/v1/tournaments/";
            string tournamentId = "";
            string participantId = "";

            //set datas for API
            if (dic.ContainsKey("{tournament}"))
            {
                tournamentId = dic["{tournament}"];
            }

            if(dic.ContainsKey("{participant_id}"))
            {
                participantId = dic["{participant_id}"];
            }

            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", jsonToken.token);
            }

            //Delete URL
            HttpResponseMessage response = await client.DeleteAsync(URL + tournamentId + "/participants/" + participantId + ".json");
            await response.Content.ReadAsStringAsync();
        }

        public static async Task StartTournament(Dictionary<string, string> dic)
        {
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();
            string tournamentId = "";

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            if (dic.ContainsKey("{tournament}"))
            {
                tournamentId = dic["{tournament}"];
            }

            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", jsonToken.token);
            }

            //set API URL
            string URL = "https://api.challonge.com/v1/tournaments/";
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);
            FormUrlEncodedContent con = new FormUrlEncodedContent(dic);

            HttpResponseMessage randomizeMessage = await client.PostAsync(URL + tournamentId + "/participants/randomize.json", content);
            HttpResponseMessage startMessage = await client.PostAsync(URL + tournamentId + "/start.json", con);
            await startMessage.Content.ReadAsStringAsync();
            await randomizeMessage.Content.ReadAsStringAsync();
        }

        public static async Task FinalizeTournament(Dictionary<string, string> dic)
        {
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();
            string tournamentId = "";

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            if (dic.ContainsKey("{tournament}"))
            {
                tournamentId = dic["{tournament}"];
            }

            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", jsonToken.token);
            }

            //set API URL
            string URL = "https://api.challonge.com/v1/tournaments/";
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);
            FormUrlEncodedContent con = new FormUrlEncodedContent(dic);

            HttpResponseMessage startMessage = await client.PostAsync(URL + tournamentId + "/finalize.json", con);
            await startMessage.Content.ReadAsStringAsync();
        }

        public static async Task AddScore(string link, int scoreJ1, int scoreJ2, string winner, string tournamentID, string matchID)
        {
            HttpClient client = new HttpClient();
            var jsonToken = new JSONReaderClass("tokenchallonge.json");
            await jsonToken.ReadJSON();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(jsonToken.prefix + ":" + jsonToken.token)));

            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (!dic.ContainsKey("{tournament}"))
            {
                dic.Add("{tournament}", tournamentID);
            }

            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", jsonToken.token);
            }

            if(!dic.ContainsKey("{match_id}"))
            {
                dic.Add("{match_id}", matchID);
            }

            if (!dic.ContainsKey("match[scores_csv]"))
            {
                dic.Add("match[scores_csv]", $"{scoreJ1}" + "-" + $"{scoreJ2}");
            }

            if (!dic.ContainsKey("match[winner_id]"))
            {
                dic.Add("match[winner_id]", winner);
            }

            //set API URL
            string URL = "https://api.challonge.com/v1/tournaments/";
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            HttpResponseMessage startMessage = await client.PutAsync(URL + link,content);
            await startMessage.Content.ReadAsStringAsync();
        }

    }
}
