using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIChallongeClass
{
    public static class QuerryBuilder
    {
        public static string GenerateGetTournamentQuerry(string prefix, string token, string subdomainName, string url, string state)
        {
            string querry = $"https://{prefix}:{token}@api.challonge.com/v1/tournaments";

            if (url == "" && subdomainName == "None")
            {
                //all tournament no subdomain and state if any
                querry = querry + ".json" + (state != "" ? $"?state={state}" : "");
            }
            else if (url == "")
            {
                //all tournament with subdomain and type if any
                querry = $"{querry}.json?subdomain={subdomainName}" + (state != "" ?  $"&state={state}" : "");
            }
            else if (subdomainName == "None")
            {
                //one specified tournament of account
                querry += "/" + url + ".json";
            }
            else
            {
                //one specified tournament of organisation
                querry += "/" + subdomainName + "-" + url + ".json";
            }

            return querry;
        }
        public static string GenerateDeleteParticipantQuerry(string prefix, string token, string subdomainName, string url, string id)
        {
            string querry = $"https://{prefix}:{token}@api.challonge.com/v1/tournaments";

            if (subdomainName == "None")
            {
                //one specified tournament of account
                querry += "/" + url + "/participant/" + id + ".json";
            }
            else
            {
                //one specified tournament of organisation
                querry += "/" + subdomainName + "-" + url + "/participant/" + id + "/" + ".json";
            }

            return querry;
        }

        public static string GeneratePutTargetQuerry(string prefix, string token, string subdomainName, string url, string target)
        {
            string querry = $"https://{prefix}:{token}@api.challonge.com/v1/tournaments";

            if (subdomainName == "None")
            {
                //one specified tournament of account
                querry += "/" + url + target + ".json";
            }
            else
            {
                //one specified tournament of organisation
                querry += "/" + subdomainName + "-" + url + target + ".json";
            }

            return querry;
        }

    }
}
