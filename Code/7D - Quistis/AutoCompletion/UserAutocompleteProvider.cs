using APIChallongeClass;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7D___Quistis.SlashCommands
{
    public class UserAutocompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provide(
        AutocompleteContext context)
        {
            // Fetch tournament data from Challonge API
            var tournamentNames = await FetchTournamentNames();

            // Convert the tournament names to DiscordApplicationCommandOptionChoice
            var options = tournamentNames
                .Select(name => new DiscordApplicationCommandOptionChoice(name, name))
                .ToList();

            return options;
        }

        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            throw new NotImplementedException();
        }

        private async Task<List<string>> FetchTournamentNames()
        {
            string result = await ConnectionChallongeAPI.GetInProgressTournament("eca83949301db430ad068e13");

            List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            List<string> myTournaments = new List<string>();

            foreach (var tournament in tournaments)
            {
                myTournaments.Add(tournament.tournament.name);
            }
            return myTournaments;
        }
    }
}
