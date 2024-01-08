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
            if(commandOptions.Count() > 0) 
            {
                return commandOptions.ToArray();
            }
            return commandOptions;
        }

        private async Task GetPendingTournament()
        {
            string result = await ConnectionChallongeAPI.GetTournamentWithState("","pending");

            List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            foreach (var tournament in tournaments)
            {
                commandOptions.Add(new DiscordApplicationCommandOptionChoice(tournament.tournament.name.ToString(), tournament.tournament.url.ToString()));
            }
        }
    }
}
