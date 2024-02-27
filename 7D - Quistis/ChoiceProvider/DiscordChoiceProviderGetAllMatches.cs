using _7D___Quistis.SlashCommands;
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
    internal class DiscordChoiceProviderGetAllMatches : IChoiceProvider
    {
        List<DiscordApplicationCommandOptionChoice> commandOptions = new List<DiscordApplicationCommandOptionChoice>();
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            await GetAllMatches();
            if(commandOptions.Count > 0)
            {
                return commandOptions.ToArray();
            }
            return commandOptions;
        }

        private async Task GetAllMatches()
        {
            ChallongeCommands cc = new ChallongeCommands();

            string result = await ConnectionChallongeAPI.GetTournamentWithState("","");

            List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            foreach (TournamentsData.Root tournament in tournaments)
            {
                if (tournament.tournament.state.ToString() == "underway" || tournament.tournament.state.ToString() == "goup_stages_underway" || tournament.tournament.state.ToString() == "ended")
                {
                    string matchesjson = await ConnectionChallongeAPI.GetMatches(tournament.tournament.url);
                    string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournament.tournament.url);

                    List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(matchesjson);

                    List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

                    Dictionary<string, string> participantsName = new Dictionary<string, string>();
                    if (tournament.tournament.state.ToString() == "underway")
                    {
                        participantsName = cc.CreateParticipantsNameList(matches, participants);
                    }
                    else
                    {
                        participantsName = cc.CreateParticipantsNameListRR(matches, participants);
                    }

                    foreach (MatchesData.Root match in matches)
                    {
                        if (match.match.state != "complete")
                        {
                            string player1name = "";
                            string player2name = "";
                            foreach (KeyValuePair<string, string> pair in participantsName)
                            {
                                if (match.match.player1_id.ToString() == pair.Value)
                                {
                                    player1name = pair.Key;
                                }
                                if (match.match.player2_id.ToString() == pair.Value)
                                {
                                    player2name = pair.Key;
                                }
                            }
                            if (player1name != "" && player2name != "")
                            {
                                commandOptions.Add(new DiscordApplicationCommandOptionChoice(
                                $"{tournament.tournament.name} : {player1name} vs {player2name} matchid: {match.match.id}", match.match.id.ToString()));
                            }
                        }
                    }
                }
            }
        }
    }
}
