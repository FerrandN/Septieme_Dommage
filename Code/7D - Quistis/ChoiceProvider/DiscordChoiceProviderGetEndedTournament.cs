using APIChallongeClass;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7D___Quistis.ChoiceProvider
{
    public class DiscordChoiceProviderGetEndedTournament : IChoiceProvider
    {
        List<DiscordApplicationCommandOptionChoice> commandOptions = new List<DiscordApplicationCommandOptionChoice>();
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            await GetAllEndedTournament();
            if(commandOptions.Count > 0)
            {
                return commandOptions.ToArray();
            }
            return commandOptions;
        }

        private async Task GetAllEndedTournament()
        {

            string result = await ConnectionChallongeAPI.GetTournamentWithState("","ended");

            List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            foreach (var tournament in tournaments)
            {
                commandOptions.Add(new DiscordApplicationCommandOptionChoice(tournament.tournament.name.ToString(), tournament.tournament.url.ToString()));
            }
        }
    }
}
