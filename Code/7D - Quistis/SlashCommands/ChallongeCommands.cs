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
using System.Threading;
using static APIChallongeClass.Participants;
using System.Xml.Linq;
using DSharpPlus.Interactivity.Extensions;
using _7D___Quistis.ChoiceProvider;
using DSharpPlus.EventArgs;

namespace _7D___Quistis.SlashCommands
{
    public class ChallongeCommands : ApplicationCommandModule
    {
        [SlashCommand("creertournoi", "Creer un tournoi")]
        public async Task CreateTournament(InteractionContext ctx,
        [Choice("single elimination","single elimination")]
        [Choice("double elimination","double elimination")]
        [Choice("round robin","round robin")]
        [Choice("swiss","swiss")]
        [Option("TypeDeTournoi", "Type du tournoi")] string type,//Type of tournament

        [Option("NomDuTournoi", "Nom du tournoi")] string name,//name of tournament

        [Option("DateDeDepart", "format: YYYY-MM-DD HH:MM:SS")] string date, //date and hours at which tournament starts

        [Option("IdDuTournoi", "l'identifiant Du Tournoi")] string url) //tournament id) 
        {
            bool haspermission = HasPermission(ctx);

            if (haspermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("tournament[start_at]", date);
                dic.Add("tournament[name]", name);
                dic.Add("tournament[url]", url);
                dic.Add("tournament[tournament_type]", type);

                if (jsonReader.subdomain != "")
                {
                    dic.Add($"tournament[subdomain]", jsonReader.subdomain);
                }

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.PostTournament(dic);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Creer Tournoi"
                    }));

                    DiscordMessageBuilder embedMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Worked perfectly")
                        .WithDescription("Tournament created at: https://challonge.com/fr/" + url));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament created at: https://challonge.com/fr/" + url, true));
                    DiscordMessageBuilder messageWithButton = new DiscordMessageBuilder();

                    messageWithButton.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary,url.ToString(), "cliquer pour rejoindre"));

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder(messageWithButton));

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

        [SlashCommand("modifiertournoi", "Modifie un tournoi")]
        public async Task UpdateTournament(InteractionContext ctx,

        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","tournament URL")] string tournamenturl)//allow group stage)
        {
            bool haspermission = HasPermission(ctx);

            if (haspermission)
            {
                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();
                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("tournament[url]", tournamenturl);

                await ctx.DeferAsync();

                string link = "";
                if (jsonReader.subdomain != "")
                {
                    link = "/" + jsonReader.subdomain + "-" + tournamenturl + ".json";
                    dic.Add("tournament[subdomain]", jsonReader.subdomain);
                }
                else
                {
                    link = "/" + tournamenturl + ".json";
                }


                try
                {
                    //send to API
                    await ConnectionChallongeAPI.UpdateTournament(dic, link);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Modifier tournoi"
                    }));

                    DiscordMessageBuilder embedMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Worked perfectly")
                        .WithDescription("Tournament created at: https://challonge.com/fr/" + tournamenturl));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament created at: https://challonge.com/fr/" + tournamenturl, true));

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

        [SlashCommand("ajoutemoi", "Ajoute l'utilisateur au tournoi")]
        public async Task AddToTournament(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("pendingtournament","tournament URL")] string tournamenturl) //tournament id) 
        {
            await ctx.DeferAsync();
            var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
            await jsonReader.ReadJSON();
            string name = ctx.User.Username;//name of participant
                                            //create element to send to API
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("participant[name]", name);
            if (jsonReader.subdomain != "")
            {
                dic.Add("{tournament}", jsonReader.subdomain + "-" + tournamenturl);
            }
            else
            {
                dic.Add("{tournament}", tournamenturl);
            }

            try
            {
                //send to API
                await ConnectionChallongeAPI.AddParticipant(dic);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Ajouter au tournoi"
                }));

                await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "You have been added to the tournament : https://challonge.com/fr/" + tournamenturl, true));

            }
            catch
            {
                await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
            }
        }

        [SlashCommand("ajoutejoueur", "Ajoute le joueur au tournoi")]
        public async Task AddSomeoneToTournament(InteractionContext ctx,
        [Option("NomJoueur", "Nom du joueur à supprimer")] DiscordUser user,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            await ctx.DeferAsync();
            var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
            await jsonReader.ReadJSON();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("participant[name]", user.Username);
            if (jsonReader.subdomain != "")
            {
                dic.Add("{tournament}", jsonReader.subdomain + "-" + tournamenturl);
            }
            else
            {
                dic.Add("{tournament}", tournamenturl);
            }

            try
            {
                //send to API
                await ConnectionChallongeAPI.AddParticipant(dic);


                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Ajouter au tournoi"
                }));


                await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "You added" + user.Username + "to the tournament : https://challonge.com/fr/" + tournamenturl, true));

            }
            catch
            {
                await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
            }
        }

        [SlashCommand("effacetournois", "Efface le tournoi")]
        public async Task DeleteTournament(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string link = SetString(jsonReader.subdomain, tournamenturl);
                dic.Add("{tournament}", link);
                try
                {
                    //send to API
                    await ConnectionChallongeAPI.DeleteTournament(dic);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Effacer tournoi"
                    }));


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

        [SlashCommand("afficheparticipants", "Affiche tout les participants inscrits au tournoi")]
        public async Task GetPlayers(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {

            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                //create element to send to API
                string link = "";
                if (jsonReader.subdomain != "None")
                {
                    link = "/" + jsonReader.subdomain + "-" + tournamenturl;
                }
                else
                {
                    link = "/" + tournamenturl;
                }

                try
                {
                    //send to API
                    string result = await ConnectionChallongeAPI.GetParticipant(link);

                    List<Participants.Root> tournaments = JsonConvert.DeserializeObject<List<Participants.Root>>(result);


                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Afficher Joueur"
                    }));


                    foreach (var participant in tournaments)
                    {
                        await ctx.Channel.SendMessageAsync(SetMessage($"Id du participant: {participant.participant.id}", $"Nom du participant: {participant.participant.name}", true));
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

        [SlashCommand("affichechaquetournois", "Affiche les informations de tout les tournois")]
        public async Task GetAllTournament(InteractionContext ctx) //tournament id)
        {
            bool hasPermission = HasPermission(ctx);

            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string result = await ConnectionChallongeAPI.GetJson(jsonReader.subdomain);

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Afficher tournois"
                    }));

                foreach (var tournament in tournaments)
                {
                    await ctx.Channel.SendMessageAsync(SetMessage($"Tournament name: {tournament.tournament.name}", $"Tournament URL: {tournament.tournament.url}", true));
                    Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("affichetournoienattente", "Affiche les informations de tout les tournois en attente d'inscription")]
        public async Task GetAllPendingTournament(InteractionContext ctx)
        {
            bool hasPermission = HasPermission(ctx);

            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string result = await ConnectionChallongeAPI.GetPendingTournament(jsonReader.subdomain);

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);


                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "afficher tournois en attente"
                }));


                foreach (var tournament in tournaments)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
                    await ctx.Channel.SendMessageAsync(SetMessage($"Tournament name: {tournament.tournament.name}", $"Tournament URL: {tournament.tournament.url}", true));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }

        }

        [SlashCommand("affichetournoiencour", "Affiche les informations de tout les tournois en cour")]
        public async Task GetAllInProgressTournament(InteractionContext ctx)
        {
            bool hasPermission = HasPermission(ctx);

            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string result = await ConnectionChallongeAPI.GetInProgressTournament(jsonReader.subdomain);

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);


                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Afficher tournois en cour"
                }));


                foreach (var tournament in tournaments)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
                    await ctx.Channel.SendMessageAsync(SetMessage($"Tournament name: {tournament.tournament.name}", $"Tournament URL: {tournament.tournament.url}", true));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }

        }

        [SlashCommand("afficheuntournoi", "Affiche les informations d'un tournoi en particulier")]
        public async Task GetTournament(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {

            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string link = "";
                if (jsonReader.subdomain != "None")
                {
                    link = "/" + jsonReader.subdomain + "-" + tournamenturl + ".json";
                }
                else
                {
                    link = "/" + tournamenturl + ".json";
                }

                string result = await ConnectionChallongeAPI.GetJson(link);

                TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(result);


                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Afficher tournoi"
                }));


                await ctx.Channel.SendMessageAsync(SetMessage($"Tournament name: {tournament.tournament.name}", $"Tournament URL: {tournament.tournament.url}", true));
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("supprimejoueur", "Supprime le joueur du tournoi")]
        public async Task RemovePlayer(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl,
        [Option("NomJoueur", "Nom du joueur à supprimer")] DiscordUser user)
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();
                
                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string link = "";
                if (jsonReader.subdomain != "None")
                {
                    link = "/" + jsonReader.subdomain + "-" + tournamenturl;
                }
                else
                {
                    link = "/" + tournamenturl;
                }
                dic.Add("{tournament}", link);
                dic.Add("{participant_id}", user.Username);

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.DeleteParticipant(dic);


                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Supprimer joueur"
                    }));


                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Player " + user.Username + " has been removed successfully from tournament " + tournamenturl, true));
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

        [SlashCommand("demarre", "Demarre le tournoi")]
        public async Task Start(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            bool haspermission = HasPermission(ctx);

            if (haspermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (jsonReader.subdomain != "None")
                {
                    dic.Add("{tournament}", jsonReader.subdomain + "-" + tournamenturl);
                }
                else
                {
                    dic.Add("{tournament}", tournamenturl);
                }


                try
                {
                    //send to API
                    await ConnectionChallongeAPI.StartTournament(dic);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Demmare tournoi"
                    }));

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

        [SlashCommand("termine", "Termine le tournoi")]
        public async Task Finalize(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            bool haspermission = HasPermission(ctx);
            if (haspermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (jsonReader.subdomain != "None")
                {
                    dic.Add("{tournament}", jsonReader.subdomain + "-" + tournamenturl);
                }
                else
                {
                    dic.Add("{tournament}", tournamenturl);
                }


                try
                {
                    //send to API
                    await ConnectionChallongeAPI.FinalizeTournament(dic);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Termine tournoi"
                    }));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament as ended, thanks everyone !", true));
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

        [SlashCommand("affichematche", "Affiche tout les matches")]
        public async Task DisplayMatches(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string link = "";
                if (jsonReader.subdomain != "None")
                {
                    link = "/" + jsonReader.subdomain + "-" + tournamenturl;
                }
                else
                {
                    link = "/" + tournamenturl;
                }

                string result = await ConnectionChallongeAPI.GetMatches(link);
                string participantsJson = await ConnectionChallongeAPI.GetParticipant(link);

                List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

                List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

                Dictionary<string, string> participantsName = CreateParticipantsNameList(matches, participants);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Affiche matches"
                }));

                int imatches = 0;

                foreach (MatchesData.Root matche in matches)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
                    imatches++;

                    string player1name = "";
                    string player2name = "";

                    foreach (KeyValuePair<string, string> pair in participantsName)
                    {
                        if (matche.match.player1_id.ToString() == pair.Value)
                        {
                            player1name = pair.Key;
                        }
                        if (matche.match.player2_id.ToString() == pair.Value)
                        {
                            player2name = pair.Key;
                        }
                    }

                    await ctx.Channel.SendMessageAsync
                        (SetMessage
                        ("Matche " + imatches,
                        $"ID: {matche.match.id}\r\n" + $"Joueur 1: {player1name}\r\n" + $"Joueur 2: {player2name}\r\n",
                        true));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("score", "Ajoute un score au tournoi")]
        public async Task AddScore(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl,
        [Option("Matchid", "Match id")] string matchid,
        [Choice("7","7")]
        [Choice("6","6")]
        [Choice("5","5")]
        [Choice("4","4")]
        [Choice("3","3")]
        [Choice("2","2")]
        [Choice("1","1")]
        [Choice("0","0")]
        [Option("Scorejoueur1", "Score joueur 1")] string scoreJ1,
        [Choice("7","7")]
        [Choice("6","6")]
        [Choice("5","5")]
        [Choice("4","4")]
        [Choice("3","3")]
        [Choice("2","2")]
        [Choice("1","1")]
        [Choice("0","0")]
        [Option("Scorejoueur2", "Score joueur 2")] string scoreJ2,
        [Option("WinnerID", "Winner ID")] DiscordUser winner)
        {
            await ctx.DeferAsync();

            var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
            await jsonReader.ReadJSON();

            string link = "";
            if (jsonReader.subdomain != "")
            {
                link = "/" + jsonReader.subdomain + "-" + tournamenturl;
            }
            else
            {
                link = "/" + tournamenturl;
            }

            string result = await ConnectionChallongeAPI.GetMatches(link);
            string participantsJson = await ConnectionChallongeAPI.GetParticipant(link);

            List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

            List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

            Dictionary<string, string> participantsName = CreateParticipantsNameList(matches, participants);

            KeyValuePair<string, string> winnerNameAndId = new KeyValuePair<string, string>();

            foreach (KeyValuePair<string, string> key in participantsName)
            {
                if (winner.Username == key.Key)
                {
                    winnerNameAndId = key;
                }
            }

            try
            {
                //send to API
                await ConnectionChallongeAPI.AddScore(link, Int32.Parse(scoreJ1), Int32.Parse(scoreJ2), winnerNameAndId.Value, tournamenturl, matchid);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Ajoute score"
                }));

                await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", $"Player: {winner} won, score was: " + (Int32.Parse(scoreJ1) > Int32.Parse(scoreJ2) ? $"{scoreJ1} - {scoreJ2}" : $"{scoreJ2} - {scoreJ1}"), true));
            }
            catch
            {
                await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
            }
        }

        [SlashCommand("affichescore", "Affiche tout les scores d'un tournoi")]
        public async Task DisplayScore(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetEndedTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            await ctx.DeferAsync();

            var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
            await jsonReader.ReadJSON();

            bool hasPermission = HasPermission(ctx);
            if (hasPermission)
            {
                //create element to send to API
                string link = "";
                if (jsonReader.subdomain != "None")
                {
                    link = "/" + jsonReader.subdomain + "-" + tournamenturl;
                }
                else
                {
                    link = "/" + tournamenturl;
                }

                try
                {
                    //send to API
                    string result = await ConnectionChallongeAPI.GetParticipant(link);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Affiche score"
                    }));

                    List<Participants.Root> tournaments = JsonConvert.DeserializeObject<List<Participants.Root>>(result);
                    tournaments.Sort((p1, p2) => Int32.Parse(p1.participant.final_rank.ToString()).CompareTo(Int32.Parse(p2.participant.final_rank.ToString())));
                    /*await ctx.Channel.SendMessageAsync($"Tournament ID: {tournaments.tournament.}");*/

                    foreach (var participant in tournaments)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
                        await ctx.Channel.SendMessageAsync(SetMessage("Score", $"Nom du participant: @{participant.participant.name} \r\n" + $"Classement du participant: {participant.participant.final_rank}", true));
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

        [SlashCommand("afficheinscription", "Affiche les boutons d'inscriptions au tournoi en cour")]
        public async Task afficheinscription(InteractionContext ctx)
        {
            bool hasPermission = HasPermission(ctx);

            if (hasPermission)
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string result = await ConnectionChallongeAPI.GetPendingTournament(jsonReader.subdomain);

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

                DiscordMessageBuilder messageWithButton = new DiscordMessageBuilder();

                //For every tournaments found, we add a button with tournament Name.
                foreach (TournamentsData.Root tournament in tournaments)
                {
                    messageWithButton.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, tournament.tournament.url.ToString(), tournament.tournament.url.ToString()));
                }

                await ctx.Channel.SendMessageAsync(messageWithButton.AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle($"Click to join")
                    ));
                await ctx.EditResponseAsync(new DiscordWebhookBuilder());
            }
            else
            {
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
        }

        [SlashCommand("test", "Affiche les boutons d'inscriptions au tournoi en cour")]
        public async Task Test(InteractionContext ctx)
        {

        }

        public bool HasPermission(InteractionContext ctx)
        {
            bool hasPermission = false;
            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "TO")
                {
                    hasPermission = true;
                }
            }
            return hasPermission;
        }
        public string SetString(string str, string url)
        {
            if (str != "")
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

        public Dictionary<string, string> CreateParticipantsNameList(List<MatchesData.Root> matches, List<Participants.Root> participants)
        {
            Dictionary<string, string> matchesList = new Dictionary<string, string>();

            foreach (MatchesData.Root match in matches)
            {
                foreach (Participants.Root participant in participants)
                {
                    if (match.match.player1_id == participant.participant.id && !matchesList.ContainsKey(participant.participant.name))
                    {
                        matchesList.Add(participant.participant.name, match.match.player1_id.ToString());
                    }
                    if (match.match.player2_id == participant.participant.id && !matchesList.ContainsKey(participant.participant.name))
                    {
                        matchesList.Add(participant.participant.name, match.match.player2_id.ToString());
                    }
                }
            }
            return matchesList;
        }

        /*
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        */
    }
}
