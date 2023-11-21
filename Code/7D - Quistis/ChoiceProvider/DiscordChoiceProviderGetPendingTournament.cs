using APIChallongeClass;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7D___Quistis.ChoiceProvider
{
    public class DiscordChoiceProviderGetPendingTournament : IChoiceProvider
    {
        List<DiscordApplicationCommandOptionChoice> commandOptions = new List<DiscordApplicationCommandOptionChoice>();
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            await GetPendingTournament();
            return commandOptions.ToArray();
        }

        private async Task GetPendingTournament()
        {
            var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
            await jsonReader.ReadJSON();

            string result = await ConnectionChallongeAPI.GetJson(jsonReader.subdomain);

            List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            foreach (var tournament in tournaments)
            {
                commandOptions.Add(new DiscordApplicationCommandOptionChoice(tournament.tournament.name.ToString(), tournament.tournament.url.ToString()));
            }
        }
    }
}
