using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static APIChallongeClass.Participants;

namespace APIChallongeClass
{
    public static class ConnectionChallongeAPI
    {
        public static HttpClient GetClientConnection(JSONReaderClass json)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(json.prefix + ":" + json.token)));
            return client;
        }

        public static async Task<string> GetTournamentWithState(string url, string state)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GenerateGetTournamentQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, state);

            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task PostTournament(Dictionary<string, string> dic)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GenerateGetTournamentQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, "", "");

            //add token to dictionnary for authentification
            if (!dic.ContainsKey("api_key"))
            {
                dic.Add("api_key", challongeJson.token);
            }
            if (subdomainJson.subdomain != "")
            {
                dic.Add($"tournament[subdomain]", subdomainJson.subdomain);
            }

            //Post datas to API
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            HttpResponseMessage response = await client.PostAsync(querry,content);
            await response.Content.ReadAsStringAsync();
        }

        public static async Task UpdateTournament(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GenerateGetTournamentQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tournament[url]", url);
            dic.Add("api_key", challongeJson.token);
            dic.Add($"tournament[subdomain]", subdomainJson.subdomain);
            if (subdomainJson.subdomain != "None")
            {
                dic.Add("{tournament}", subdomainJson.subdomain + "-" + url);
            }
            else
            {
                dic.Add("{tournament}", url);
            }

            string tournament = await GetTournament(url);
            TournamentData.Root t = JsonConvert.DeserializeObject<TournamentData.Root>(tournament);
            if (t.tournament.tournament_type == "round robin" && t.tournament.state == "pending")
            {
                dic.Add("tournament[state]", "start_group_stage");
            }


            //Post datas to API
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            HttpResponseMessage response = await client.PutAsync(querry, content);
            await response.Content.ReadAsStringAsync();
        }

        public static async Task AddParticipant(string url, string username)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("api_key", challongeJson.token);
            dic.Add("participant[name]", username);
            if (subdomainJson.subdomain != "")
            {
                dic.Add("{tournament}", subdomainJson.subdomain + "-" + url);
            }
            else
            {
                dic.Add("{tournament}", url);
            }
            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "/participants");
            //set datas for API
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);
            //Post datas to API
            HttpResponseMessage response = await client.PostAsync(querry, content);
            await response.Content.ReadAsStringAsync();
        }

        public static async Task DeleteTournament(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GenerateGetTournamentQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "");

            //Delete URL
            HttpResponseMessage response = await client.DeleteAsync(querry);
            await response.Content.ReadAsStringAsync();
            await Task.CompletedTask;
        }
        public static async Task<string> GetTournament(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client =  GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GenerateGetTournamentQuerry(challongeJson.prefix, challongeJson.token,subdomainJson.subdomain, url, "");

            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<string> GetMatches(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "/matches") ;

            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetMatch(string url, string id)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, $"/matches/{id}");

            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetParticipant(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "/participants");

            HttpResponseMessage response = await client.GetAsync(querry);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task DeleteParticipant(string url, string id)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tournament[url]", url);
            dic.Add("api_key", challongeJson.token);
            dic.Add($"tournament[subdomain]", subdomainJson.subdomain);

            await RandomizeTournament(url);

            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            string querry = QuerryBuilder.GenerateDeleteParticipantQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, id);
            HttpResponseMessage startMessage = await client.PostAsync(querry, content);

            await startMessage.Content.ReadAsStringAsync();
        }

        public static async Task StartTournament(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tournament[url]", url);
            dic.Add("api_key", challongeJson.token);
            dic.Add($"tournament[subdomain]", subdomainJson.subdomain);

            await RandomizeTournament(url);

            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "/start");
            HttpResponseMessage startMessage = await client.PostAsync(querry, content);

            await startMessage.Content.ReadAsStringAsync();
        }

        public static async Task RandomizeTournament(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tournament[url]", url);
            dic.Add("api_key", challongeJson.token);
            dic.Add($"tournament[subdomain]", subdomainJson.subdomain);

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "/participants/randomize");

            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            HttpResponseMessage randomizeMessage = await client.PostAsync(querry, content);
            await randomizeMessage.Content.ReadAsStringAsync();
        }

        public static async Task FinalizeTournament(string url)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("tournament[url]", url);
            dic.Add("api_key", challongeJson.token);
            dic.Add($"tournament[subdomain]", subdomainJson.subdomain);

            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, url, "/finalize");
            HttpResponseMessage finalizeMessage = await client.PostAsync(querry, content);

            await finalizeMessage.Content.ReadAsStringAsync();
        }

        public static async Task AddScore(string tournamentID, string matchID, int scoreJ1, int scoreJ2, string winner)
        {
            var challongeJson = new JSONReaderClass("tokenchallonge.json");
            await challongeJson.ReadJSON();

            HttpClient client = GetClientConnection(challongeJson);

            var subdomainJson = new JSONReaderSubdomainClass("subdomain.json");
            await subdomainJson.ReadJSON();

            string participantsJson = await GetParticipant(tournamentID);
            List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

            string pid = "";
            foreach (Participants.Root p in participants)
            {
                if(winner == p.participant.name)
                {
                    pid = p.participant.id.ToString();
                }
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("{tournament}", tournamentID);
            dic.Add("api_key", challongeJson.token);
            dic.Add("{match_id}", matchID);
            dic.Add("match[scores_csv]", $"{scoreJ1}" + "-" + $"{scoreJ2}");
            dic.Add("match[winner_id]", pid);

            string querry = QuerryBuilder.GeneratePutTargetQuerry(challongeJson.prefix, challongeJson.token, subdomainJson.subdomain, tournamentID, $"/matches/{matchID}");
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);

            HttpResponseMessage startMessage = await client.PutAsync(querry,content);
            await startMessage.Content.ReadAsStringAsync();
        }

        public static async Task Test()
        {
            
        }
    }
}
