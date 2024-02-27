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
    internal class DiscordChoiceProviderGetInProgressTournament : IChoiceProvider
    {
        List<DiscordApplicationCommandOptionChoice> commandOptions = new List<DiscordApplicationCommandOptionChoice>();
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            await GetPendingAllInProgressTournament();
            if(commandOptions.Count > 0)
            {
                return commandOptions.ToArray();
            }
            return commandOptions;
        }

        private async Task GetPendingAllInProgressTournament()
        {
            string result = await ConnectionChallongeAPI.GetTournamentWithState("","");
            List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            foreach (var tournament in tournaments)
            {
                if(tournament.tournament.state.ToString() == "underway" || tournament.tournament.state.ToString() == "group_stages_underway" || tournament.tournament.state.ToString() == "awaiting_review")
                {
                    commandOptions.Add(new DiscordApplicationCommandOptionChoice(tournament.tournament.name.ToString(), tournament.tournament.url.ToString()));
                }
                
            }
        }
    }
}
