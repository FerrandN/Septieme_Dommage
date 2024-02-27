using APIChallongeClass;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace _7D___Quistis.commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("createTournament", "Create a tournament")]
        public async Task CreateTournament(InteractionContext ctx,
        [Choice("single elimination","single elimination")]
        [Choice("double elimination","double elminination")]
        [Choice("round robin","round robin")]
        [Choice("swiss","swiss")]
        [Option("TournamentType", "Type of Tournament")] string type,//Type of tournament

        [Choice("No subdomain", "None")]
        [Choice("Nekoyuki Subdomain", "eca83949301db430ad068e13")]
        [Option("SubDomain", "Your subdomain")] string subd,//name of tournament

        [Option("TournamentName","Tournament name")]string name,//name of tournament

        [Option("StartDate", "format: YYYY-MM-DD HH:MM:SS")] string date, //date and hours at which tournament starts

        [Option("TournamentURL", "Tournament URL")] string url) //tournament id) 
        {
            bool haspermission = HasPermission(ctx);

            if(haspermission)
            {

                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("tournament[start_at]", date);
                dic.Add("tournament[name]", name);
                dic.Add("tournament[url]", url);
                dic.Add("tournament[tournament_type]", type);
                if (subd != "None")
                {
                    dic.Add("tournament[subdomain]", subd);
                }

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.PostTournament(dic);

                    DiscordMessageBuilder embedMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Worked perfectly")
                        .WithDescription("Tournament created at: https://challonge.com/fr/" + url));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament created at: https://challonge.com/fr/" + url, true));

                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }

            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("addMe", "Add user to linked tournament")]
        public async Task AddToTournament(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain","eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("SubDomain", "Your Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url) //tournament id) 
        {
            string name = ctx.User.Username;//name of participant
                                            //create element to send to API
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("participant[name]", name);
            if (sub != "None")
            {
                dic.Add("{tournament}", sub + "-" + url);
            }
            else
            {
                dic.Add("{tournament}", url);
            }

            try
            {
                //send to API
                await ConnectionChallongeAPI.AddParticipant(dic);
                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "You have been added to the tournament : https://challonge.com/fr/" + url, true));

            }
            catch
            {
                await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
            }
        }

        [SlashCommand("deletetournament", "Delete tournament")]
        public async Task DeleteTournament(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain","eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("Subdomain", "Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url) //tournament id) 
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string link = SetString(sub,url);
                dic.Add("{tournament}", link);

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.DeleteTournament(dic);
                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament has been deleted successfully", true));
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("getparticipant", "Get all registered players from one tournament")]
        public async Task GetPlayers(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain","eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("Subdomain", "Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url) //tournament id) 
        {

            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                //create element to send to API
                string link = "";
                if (sub != "None")
                {
                    link = "/" + sub + "-" + url;
                }
                else
                {
                    link = "/" + url;
                }

                try
                {
                    //send to API
                    string result = await ConnectionChallongeAPI.GetParticipant(link);

                    List<Participants.Root> tournaments = JsonConvert.DeserializeObject<List<Participants.Root>>(result);

                    /*await ctx.Channel.SendMessageAsync($"Tournament ID: {tournaments.tournament.}");*/

                    foreach (var participant in tournaments)
                    {
                        await ctx.Channel.SendMessageAsync($"Nom du participants: {participant.participant.name}");
                        await ctx.Channel.SendMessageAsync($"Id du participants: {participant.participant.id}");
                    }
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("getalltournament", "Get all tournament from account or subdomain")]
        public async Task GetAllTournament(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain","eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("SubDomain", "Your Subdomain")] string subd)
        {
            bool hasPermission = HasPermission(ctx);

            if(hasPermission)
            {
                string result = await ConnectionChallongeAPI.GetJson(subd);

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

                foreach (var tournament in tournaments)
                {
                    await ctx.Channel.SendMessageAsync($"Tournament ID: {tournament.tournament.id}");
                    await ctx.Channel.SendMessageAsync($"Tournament name: {tournament.tournament.name}");
                    await ctx.Channel.SendMessageAsync($"Tournament URL: {tournament.tournament.url}");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }

        }

        [SlashCommand("gettournament", "Get tournament from account or subdomain with ID or name")]
        public async Task GetTournament(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain","eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("SubDomain", "Your Subdomain")] string subd,
        [Option("TournamentURL", "Tournament URL")] string url)
        {
            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                string link = "";
                if (subd != "None")
                {
                    link = "/" + subd + "-" + url + ".json";
                }
                else
                {
                    link = "/" + url + ".json";
                }

                string result = await ConnectionChallongeAPI.GetJson(link);

                TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(result);

                await ctx.Channel.SendMessageAsync($"Tournament ID: {tournament.tournament.id}");
                await ctx.Channel.SendMessageAsync($"Tournament name: {tournament.tournament.name}");
                await ctx.Channel.SendMessageAsync($"Tournament Lien: https://challonge.com/fr/{tournament.tournament.url}");
                await ctx.Channel.SendMessageAsync($"Tournament Description: {tournament.tournament.description}");
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("removeplayer", "Remove player from linked URL tournament")]
        public async Task RemovePlayer(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain", "eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("Subdomain", "Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url,
        [Option("PlayerID","Player ID")]string pID)
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string link = "";
                if (sub != "None")
                {
                    link = "/" + sub + "-" + url;
                }
                else
                {
                    link = "/" + url;
                }
                dic.Add("{tournament}", link);
                dic.Add("{participant_id}", pID);

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.DeleteParticipant(dic);
                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly","Player " + pID + " has been removed successfully from tournament " + url, true));
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("start", "Start linked tournament")]
        public async Task Start(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain", "eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("Subdomain", "Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url)
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                if(sub != "None")
                {
                    dic.Add("{tournament}",sub + "-" + url);
                }
                else
                {
                    dic.Add("{tournament}", url);
                }
                

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.StartTournament(dic);

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament as started !! GL (Monks are the bests)", true));
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }

            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("finalize", "End tournament")]
        public async Task Finalize(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain", "eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("Subdomain", "Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url)
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (sub != "None")
                {
                    dic.Add("{tournament}", sub + "-" + url);
                }
                else
                {
                    dic.Add("{tournament}", url);
                }


                try
                {
                    //send to API
                    await ConnectionChallongeAPI.FinalizeTournament(dic);

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament as started !! GL (Monks are the bests)", true));
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }

            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("displaymatches", "Display all marches")]
        public async Task DisplayMatches(InteractionContext ctx,
        [Choice("Nekoyuki Subdomain", "eca83949301db430ad068e13")]
        [Choice("No subdomain", "None")]
        [Option("Subdomain", "Subdomain")]string sub,
        [Option("TournamentURL", "Tournament URL")] string url)
        {
            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                string link = "";
                if (sub != "None")
                {
                    link = "/" + sub + "-" + url;
                }
                else
                {
                    link = "/" + url;
                }

                string result = await ConnectionChallongeAPI.GetTournamentWithMatches(link);

                List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);
                int imatches = 0;
                foreach (MatchesData.Root matche in matches)
                {
                    imatches++;
                    DiscordMessageBuilder embedMessage = new DiscordMessageBuilder();
                    await ctx.Channel.SendMessageAsync(
                    embedMessage.AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Blue)
                    .WithTitle("Matche " + imatches)
                    .WithDescription($"ID: {matche.match.tournament_id}\r\n" + $"Joueur 1: {matche.match.player1_id}\r\n" + $"Joueur 2: {matche.match.player2_id}\r\n")));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }
        public bool HasPermission(InteractionContext ctx)
        {
            bool hasPermission = false;
            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "7ème dommage")
                {
                    hasPermission = true;
                }
            }
            return hasPermission;
        }
       public string SetString(string str, string url)
        {
            if (str != "None")
            {
                str = str + "-" + url + ".json";
            }
            else
            {
                str = url + ".json";
            }
            return str; 
        }

        public DiscordMessageBuilder SetMessage(string title, string message, bool isSuccessfull)
        {
            DiscordMessageBuilder embedMessage = new DiscordMessageBuilder();
            return embedMessage.AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(isSuccessfull ? DiscordColor.Blue : DiscordColor.Red)
                        .WithTitle(title)
                        .WithDescription(message));
        }


        /*
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        */
    }
}
