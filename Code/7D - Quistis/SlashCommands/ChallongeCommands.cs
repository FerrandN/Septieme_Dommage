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
using System.ComponentModel;
using _7D___Quistis.DataBase;
using Npgsql;
using System.Windows.Forms;
using System.Windows;
using static System.Windows.Forms.LinkLabel;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.IO.Pipes;
using JWTManager;
using System.Net;
using System.Net.Http.Headers;

namespace _7D___Quistis.SlashCommands
{
    public class ChallongeCommands : ApplicationCommandModule
    {
        [SlashCommand("Creer_tournoi", "Creer un tournoi")]
        public async Task CreateTournament(InteractionContext ctx,
        [Choice("single elimination","single elimination")]
        [Choice("double elimination","double elimination")]
        [Choice("round robin","round robin")]
        [Choice("swiss","swiss")]
        [Option("TypeDeTournoi", "Type du tournoi")] string type,//Type of tournament

        [Option("NomDuTournoi", "Nom du tournoi")] string name,//name of tournament
        [Choice("2024-12-24 18:30:00","2024-12-24 18:30:00")]
        [Option("DateDeDepart", "format: YYYY-MM-DD HH:MM:SS")] string date, //date and hours at which tournament starts

        [Option("IdDuTournoi", "l'identifiant Du Tournoi")] string url) //tournament id) 
        {
            if (await HasPermission(ctx))
            {
                //tell discord to wait for response
                await ctx.DeferAsync();

                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("tournament[start_at]", date);
                dic.Add("tournament[name]", name);
                dic.Add("tournament[url]", url);
                dic.Add("tournament[tournament_type]", type);

                //add round robin datas
                if (type == "round robin")
                {
                    dic.Add("tournament[group_stages_enabled]", "true");
                    // à modifier si on veux plus ou moin de joueur par pool
                    dic.Add("tournament[participants_count]", "4");
                }

                try
                {
                    //ask datas from api
                    await ConnectionChallongeAPI.PostTournament(dic);

                    //add embed
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Creer Tournoi"
                    }));

                    //store data in DB
                    await DBEngine.PostTournament(type.Replace(' ', '_'), name);


                    //display info if everything is ok
                    DiscordMessageBuilder embedMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Worked perfectly")
                        .WithDescription("Tournament created at: https://challonge.com/fr/" + url));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament created at: https://challonge.com/fr/" + url, true));
                    DiscordMessageBuilder messageWithButton = new DiscordMessageBuilder();
                    
                    messageWithButton.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary,"RegistrationButton", $"{url}"));
                    DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(date);
                    await ctx.Guild.CreateEventAsync("Tournoi: " + name, type,null,ScheduledGuildEventType.External,ScheduledGuildEventPrivacyLevel.GuildOnly, dateTimeOffset,dateTimeOffset.AddHours(5),"Discord 7D");
                    
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder(messageWithButton));

                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
        }

        [SlashCommand("Modifier_tournoi", "Modifie un tournoi")]
        public async Task UpdateTournament(InteractionContext ctx,

        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","tournament URL")] string tournamenturl)//allow group stage)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.UpdateTournament(tournamenturl);

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
        }

        [SlashCommand("Ajoute_moi", "Ajoute l'utilisateur au tournoi")]
        public async Task AddToTournament(InteractionContext ctx,
        [Option("DeckList", "Decklist")] string decklist,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("pendingtournament","tournament URL")] string tournamenturl) //tournament id) 
        {
            await ctx.DeferAsync();
            try
            {
                //send to API
                await ConnectionChallongeAPI.AddParticipant(tournamenturl,ctx.User.Username);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Ajouter au tournoi"
                }));

                bool addedToDB = await DBEngine.StorePlayer(ctx.User.Username);
                await DBEngine.PostRegistration(decklist, tournamenturl, ctx.User.Username);


                if (addedToDB)
                {
                    await ctx.Channel.SendMessageAsync($"{ctx.User.Username} a effectué sa première inscription !");
                }

                await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "You have been added to the tournament : https://challonge.com/fr/" + tournamenturl, true));

            }
            catch
            {
                await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
            }
        }

        [SlashCommand("Ajoute_joueur", "Ajoute le joueur au tournoi")]
        public async Task AddSomeoneToTournament(InteractionContext ctx,
        [Option("NomJoueur", "Nom du joueur à ajouter")] DiscordUser user,
        [Option("DeckList","Decklist")]string decklist,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            if(await HasPermission(ctx))
            {
                await ctx.DeferAsync();
                try
                {
                    //send to API
                    await ConnectionChallongeAPI.AddParticipant(tournamenturl, user.Username);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Ajouter au tournoi"
                    }));

                    bool addedToDB = await DBEngine.StorePlayer(user.Username);
                    await DBEngine.PostRegistration(decklist, tournamenturl, user.Username);

                    if (addedToDB)
                    {
                        await ctx.Channel.SendMessageAsync($"{ctx.User.Username} a effectué sa première inscription !");
                    }

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "You added " + user.Username + " to the tournament : https://challonge.com/fr/" + tournamenturl, true));

                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
        }

        [SlashCommand("Efface_tournois", "Efface le tournoi")]
        public async Task DeleteTournament(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();
                try
                {
                    //send to API
                    await ConnectionChallongeAPI.DeleteTournament(tournamenturl);

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
        }

        [SlashCommand("Affiche_participants", "Affiche tout les participants inscrits au tournoi")]
        public async Task GetPlayers(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                try
                {
                    //send to API
                    string result = await ConnectionChallongeAPI.GetParticipant(tournamenturl);

                    List<Participants.Root> tournaments = JsonConvert.DeserializeObject<List<Participants.Root>>(result);


                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Afficher Joueur"
                    }));


                    foreach (var participant in tournaments)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        await ctx.Channel.SendMessageAsync(SetMessage($"Id du participant: {participant.participant.id}", $"Nom du participant: {participant.participant.name}", true));
                    }
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
        }

        [SlashCommand("Affiche_chaque_tournois", "Affiche les informations de tout les tournois")]
        public async Task GetAllTournament(InteractionContext ctx) //tournament id)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                string result = await ConnectionChallongeAPI.GetTournament("");

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
        }

        [SlashCommand("Affiche_tournoi_en_attente", "Affiche les informations de tout les tournois en attente d'inscription")]
        public async Task GetAllPendingTournament(InteractionContext ctx)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                string result = await ConnectionChallongeAPI.GetTournamentWithState("","pending");

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
        }

        [SlashCommand("Affiche_tournoi_en_cour", "Affiche les informations de tout les tournois en cour")]
        public async Task GetAllInProgressTournament(InteractionContext ctx)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                string result = await ConnectionChallongeAPI.GetTournamentWithState("","in_progress");

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
        }

        [SlashCommand("Affiche_un_tournoi", "Affiche les informations d'un tournoi en particulier")]
        public async Task GetTournament(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                string result = await ConnectionChallongeAPI.GetTournament(tournamenturl);

                TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(result);


                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Afficher tournoi"
                }));

                try
                {
                    await ctx.Channel.SendMessageAsync(SetMessage($"Tournament name: {tournament.tournament.name}", $"Tournament URL: {tournament.tournament.url}", true));
                    await ctx.Channel.SendMessageAsync($"state: {tournament.tournament.state}");
                }
                catch (Exception ex)
                {
                    await ctx.Channel.SendMessageAsync($"Erreur, merci de contacter @Nekoyuki");
                }
            }
        }

        [SlashCommand("Efface_joueur", "Supprime le joueur du tournoi")]
        public async Task RemovePlayer(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl,
        [Option("NomJoueur", "Nom du joueur à supprimer")] DiscordUser user)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.DeleteParticipant(tournamenturl,user.Username);


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
        }

        [SlashCommand("Demarre", "Demarre le tournoi")]
        public async Task Start(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                try
                {
                    await ConnectionChallongeAPI.StartTournament(tournamenturl);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Demare tournoi"
                    }));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament as started !! GL (Monks are the bests)", true));
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
        }

        [SlashCommand("Termine", "Termine le tournoi")]
        public async Task Finalize(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetInProgressTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                try
                {
                    //send to API
                    await ConnectionChallongeAPI.FinalizeTournament(tournamenturl);

                    string result = await ConnectionChallongeAPI.GetParticipant(tournamenturl);

                    List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(result);

                    foreach( Participants.Root participant in participants )
                    {
                        await AddTournamentPoins(participant.participant, participants.Count, tournamenturl);
                    }

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
        }

        [SlashCommand("Affiche_matche", "Affiche tout les matches")]
        public async Task DisplayMatches(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetInProgressTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
                string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
                string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

                List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

                List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

                TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

                Dictionary<string, string> participantsName = new Dictionary<string, string>();

                if (tournament.tournament.state == "group_stages_underway")
                {
                    participantsName = CreateParticipantsNameListRR(matches, participants);
                }
                else
                {
                    participantsName = CreateParticipantsNameList(matches, participants);
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = "Affiche matches"
                }));

                int imatches = 0;

                foreach (MatchesData.Root matche in matches)
                {
                    imatches++;

                    if (matche.match.state == "open")
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
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
                        await DBEngine.PostMatch(matche.match.id, tournamenturl, player1name, player2name);
                        await ctx.Channel.SendMessageAsync
                            (SetMessage
                            ("Matche " + imatches,
                            $"ID: {matche.match.id}\r\n" + $"Joueur 1: {player1name}\r\n" + $"Joueur 2: {player2name}\r\n",
                            true));
                    }
                }
            }
        }

        [SlashCommand("Score", "Ajoute un score au tournoi")]
        public async Task AddScore(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetInProgressTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            await ctx.DeferAsync();

            string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
            string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
            string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

            List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

            List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

            TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

            Dictionary<string, string> participantsName = new Dictionary<string, string>();

            List<MatchesData.Root> playerMatches = new List<MatchesData.Root>();

            List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();

            //Ajouter les matches du joueur dans la liste "matche"
            if(tournament.tournament.tournament_type != "round robin")
            {
                //on récupère les infos du joueur
                Participants.Root player = ReturnParticipants(ctx.User.Username,participants);

                foreach(MatchesData.Root match in matches)
                {
                    if(match.match.state != "complete")
                    {
                        if (match.match.player1_id == player.participant.id || match.match.player2_id == player.participant.id)
                        {
                            playerMatches.Add(match);
                        }
                    }
                }

                participantsName = CreateParticipantsNameList(playerMatches, participants);

                foreach(MatchesData.Root match in playerMatches)
                {
                    string player1Name = "";
                    string player2Name = "";

                    foreach(Participants.Root participant in participants)
                    {
                        if(participant.participant.id == match.match.player1_id)
                        {
                            player1Name = participant.participant.name;
                        }
                        if (participant.participant.id == match.match.player2_id)
                        {
                            player2Name = participant.participant.name;
                        }
                    }
                    matchListOption.Add(new DiscordSelectComponentOption($"tournoi {tournamenturl} : {player1Name} vs {player2Name}", $"{tournamenturl} {player1Name} {player2Name} {match.match.id}"));
                }
            }
            else
            {
                //on récupère les infos du joueur
                Participants.Root player = ReturnParticipants(ctx.User.Username, participants);

                foreach (MatchesData.Root match in matches)
                {
                    if (match.match.player1_id.ToString() == player.participant.group_player_ids.First().ToString() || match.match.player2_id.ToString() == player.participant.group_player_ids.First().ToString())
                    {
                        playerMatches.Add(match);
                    }
                }

                participantsName = CreateParticipantsNameList(playerMatches, participants);

                foreach (MatchesData.Root match in playerMatches)
                {
                    string player1Name = "";
                    string player2Name = "";

                    foreach (Participants.Root participant in participants)
                    {
                        if (participant.participant.group_player_ids.First().ToString() == match.match.player1_id.ToString())
                        {
                            player1Name = participant.participant.name;
                        }
                        if (participant.participant.group_player_ids.First().ToString() == match.match.player2_id.ToString())
                        {
                            player2Name = participant.participant.name;
                        }
                    }
                    matchListOption.Add(new DiscordSelectComponentOption($"tournoi {tournamenturl} : {player1Name} vs {player2Name}", $"{tournamenturl} {player1Name} {player2Name} {match.match.id}"));
                }
            }
            await SendDropDownList(matchListOption, ctx);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder());
        }

        [SlashCommand("Affiche_score", "Affiche tout les scores d'un tournoi")]
        public async Task DisplayScore(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetEndedTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl) //tournament id) 
        {
            await ctx.DeferAsync();

            if (await HasPermission(ctx))
            {
                try
                {
                    //send to API
                    string result = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
                    string t = await ConnectionChallongeAPI.GetTournament(tournamenturl);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Affiche score"
                    }));
                    TournamentData.Root td = JsonConvert.DeserializeObject<TournamentData.Root>(t);
                    List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(result);
                    if(participants.Count > 0)
                    {
                        if(td.tournament.tournament_type != "round robin")
                        participants.Sort((p1, p2) => Int32.Parse(p1.participant.final_rank.ToString()).CompareTo(Int32.Parse(p2.participant.final_rank.ToString())));
                        else
                        {
                            foreach(Participants.Root p in participants)
                            {
                                if(p.participant.final_rank == null)
                                {
                                    p.participant.final_rank = participants.Count;
                                }
                            }
                            participants.Sort((p1, p2) => Int32.Parse(p1.participant.final_rank.ToString()).CompareTo(Int32.Parse(p2.participant.final_rank.ToString()))); 
                        }
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(SetMessage("Error", "Not enough player", false));
                    }

                    foreach (var participant in participants)
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
        }

        [SlashCommand("Affiche_inscription", "Affiche les boutons d'inscriptions au tournoi en cour")]
        public async Task afficheinscription(InteractionContext ctx)
        {
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync();

                var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
                await jsonReader.ReadJSON();

                string result = await ConnectionChallongeAPI.GetTournamentWithState("","pending");

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

                DiscordMessageBuilder messageWithButton = new DiscordMessageBuilder();

                //For every tournaments found, we add a button with tournament Name.
                foreach (TournamentsData.Root tournament in tournaments)
                {
                    messageWithButton.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, tournament.tournament.url.ToString(), tournament.tournament.url.ToString()));
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder(messageWithButton));
            }
        }


        [SlashCommand("Mes_matches", "Affiche tout les matches")]
        public async Task DisplayMyMatches(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetPendingTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            await ctx.DeferAsync();

            string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
            string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
            string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

            List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

            List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

            TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

            Dictionary<string, string> participantsName = new Dictionary<string, string>();

            if (tournament.tournament.state == "group_stages_underway")
            {
                participantsName = CreateParticipantsNameListRR(matches, participants);
            }
            else
            {
                participantsName = CreateParticipantsNameList(matches, participants);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
            {
                Title = "Affiche matches"
            }));

            int imatches = 0;

            foreach (MatchesData.Root matche in matches)
            {
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

                if (player1name == ctx.User.Username || player2name == ctx.User.Username)
                {
                    await DBEngine.PostMatch(matche.match.id, tournamenturl, player1name, player2name);

                    await ctx.Channel.SendMessageAsync
                        (SetMessage
                        ("Matche " + imatches,
                        $"ID: {matche.match.id}\r\n" + $"Joueur 1: {player1name}\r\n" + $"Joueur 2: {player2name}\r\n",
                        true));
                    Thread.Sleep(TimeSpan.FromSeconds(2)); //cooldown the request per sec
                }
            }
        }

        [SlashCommand("Refresh", "Affiche les boutons d'inscriptions au tournoi en cour")]
        public async void Refresh(InteractionContext ctx)
        {
            await Program.Client.GetSlashCommands().RefreshCommands();
        }

        [SlashCommand("ChangePoint", "change Point")]
        public async Task ChangePoint(InteractionContext ctx,
            [Option("points","points")] string userInput,
            [Option("joueur","joueur")] DiscordUser user)
        {
            if(await HasPermission(ctx))
            {
                int points = Int32.Parse(userInput);
                await DBEngine.ChangePlayerPoints(points, user.Username);
                if (points > 0)
                {
                    await ctx.Channel.SendMessageAsync($"l'utilisateur {user.Username} a gagné {points}");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"{points} ont été retiré a l'utilisateur {user.Username}, villain !");
                }
            }

        }

        [SlashCommand("ChangeNomPrenom", "Change Nom Prenom")]
        public async Task ChangePlayerNameAndSurname(InteractionContext ctx,
            [Option("Nom", "Nom")] string surname,
            [Option("Prénom", "Prénom")] string name,
            [Option("joueur", "joueur")] DiscordUser user)
        {
            if(await HasPermission(ctx))
            {
                await ctx.DeferAsync();
                await DBEngine.ChangePlayerNameAndUsername(surname, name, ctx.User.Username);
                await ctx.Channel.SendMessageAsync($"l'utilisateur {user.Username} a correctement été nommé {surname} {name}");
            }

        }

        [SlashCommand("Test", "Test")]
        [RequirePermissions(Permissions.Administrator, true)]
        public async Task Test(InteractionContext ctx)
        {
            string requestBody = "{\"client_id\":\"a192729b4fdd3d07ae9b4760f480cf14b50ad4d88cfe661adf39c2ecba1184ac\",\"scope\":\"me tournaments:read matches:read participants:read\" }";
            string jwt = await CreateJWT.GenerateJWT(requestBody);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Signature", jwt);

                //Device Grant Authorization Request
                //HttpResponseMessage response = await client.PostAsync("https://auth.challonge.com/oauth/authorize_device", new StringContent(requestBody));

                //Grant Request with community
                HttpResponseMessage response = await client.GetAsync("https://api.challonge.com/oauth/authorize?scope=me tournaments:read matches:read participants:read&client_id=a192729b4fdd3d07ae9b4760f480cf14b50ad4d88cfe661adf39c2ecba1184ac&redirect_uri=&response_type=code&community_id=eca83949301db430ad068e13");
                if (response.IsSuccessStatusCode)
                {
                    string code = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Request succeeded!");
                    Console.WriteLine(code);
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
            }
        }

        [SlashCommand("Test2", "Test2")]
        [RequirePermissions(Permissions.Administrator, true)]
        public async Task Test2(InteractionContext ctx)
        {
            //Token Request
            string requestBody = "{\"grant_type\":\"authorization_code\",\"code\":\"39467\", \"client_id\":\"a192729b4fdd3d07ae9b4760f480cf14b50ad4d88cfe661adf39c2ecba1184ac\" }";
            string jwt = await CreateJWT.GenerateJWT(requestBody);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Signature", jwt);

                HttpResponseMessage response = await client.PostAsync("https://api.challonge.com/oauth/token", new StringContent(requestBody));

                if (response.IsSuccessStatusCode)
                {
                    string code = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Request succeeded!");
                    Console.WriteLine(code);
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
            }
        }

        public static async Task DisplayRegistrationModal(string tournamentName, string playerName, ComponentInteractionCreateEventArgs e)
        {
            var modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Enregistrement tournoi")
            .WithCustomId("RegistrationTournament")
            .AddComponents(new TextInputComponent("Pseudo", "Name", "pseudo", playerName))
            .AddComponents(new TextInputComponent("Tournois", "Tournois", "Tournois", tournamentName))
            .AddComponents(new TextInputComponent("DeckList", "ffdecks", "HTTP://FFDECKS",null,false,TextInputStyle.Short));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
        public static async Task DisplayScoreModal(string tournamentName,string player1, string player2, string matchId, ComponentInteractionCreateEventArgs e)
        {
            var modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Enregistrement tournoi")
            .WithCustomId("Score")
            .AddComponents(new TextInputComponent("Tournois", "Tournois", "Tournois", tournamentName))
            .AddComponents(new TextInputComponent("Match ID", "matchid", "", matchId))
            .AddComponents(new TextInputComponent("score " + player1, player1, "0-7"))
            .AddComponents(new TextInputComponent("score " + player2, player2, "0-7"))
            .AddComponents(new TextInputComponent("winner (supprimer le perdant)","winner", $"{player1} {player2}", $"{player1} {player2}"));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }

        public async Task<bool> HasPermission(InteractionContext ctx)
        {
            bool hasPermission = false;
            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "TO" || role.Name == "7ème dommage")
                {
                    hasPermission = true;
                }
            }
            if(hasPermission == false)
            {                      
                await ctx.Channel.SendMessageAsync("vous n'avez pas l'autorisation d'utiliser les commandes");
            }
            return hasPermission;
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
                    //group player ID pour les round robin
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

        public Dictionary<string, string> CreateParticipantsNameListRR(List<MatchesData.Root> matches, List<Participants.Root> participants)
        {
            Dictionary<string, string> matchesList = new Dictionary<string, string>();

            foreach (MatchesData.Root match in matches)
            {
                foreach (Participants.Root participant in participants)
                {
                    //group player ID pour les round robin
                    if (match.match.player1_id == (long)participant.participant.group_player_ids.First() && !matchesList.ContainsKey(participant.participant.name))
                    {
                        matchesList.Add(participant.participant.name, match.match.player1_id.ToString());
                    }
                    if (match.match.player2_id == (long)participant.participant.group_player_ids.First() && !matchesList.ContainsKey(participant.participant.name))
                    {
                        matchesList.Add(participant.participant.name, match.match.player2_id.ToString());
                    }
                }
            }
            return matchesList;
        }

        public async Task AddTournamentPoins(Participant p, int count, string tournamentname)
        {
            if(count > 24)
            {
                await ComputeScore25p(p, tournamentname);
            }
            else if(count > 8)
            {
                await ComputeScore24p(p, tournamentname);
            }
            else
            {
                await ComputeScore8p(p, tournamentname);
            }
        }

        public async Task ComputeScore8p(Participant p, string tournamentname)
        {
            if(Int32.Parse(p.final_rank.ToString()) == 1)
            {
                await DBEngine.AddTournamentPoints(5, p.display_name, tournamentname);
            }
            else if(Int32.Parse(p.final_rank.ToString()) == 2)
            {
                await DBEngine.AddTournamentPoints(3, p.display_name, tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) == 3 || Int32.Parse(p.final_rank.ToString()) == 4 )
            {
                await DBEngine.AddTournamentPoints(2, p.display_name, tournamentname);
            }
            else
            {
                await DBEngine.AddTournamentPoints(1, p.display_name, tournamentname);
            }
        }
        public async Task ComputeScore24p(Participant p, string tournamentname)
        {
            if (Int32.Parse(p.final_rank.ToString()) == 1)
            {
                await DBEngine.AddTournamentPoints(7, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) == 2)
            {
                await DBEngine.AddTournamentPoints(5, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) == 3 || Int32.Parse(p.final_rank.ToString()) == 4)
            {
                await DBEngine.AddTournamentPoints(3, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) <= 8)
            {
                await DBEngine.AddTournamentPoints(2, p.username.ToString(), tournamentname);
            }
            else
            {
                await DBEngine.AddTournamentPoints(1, p.username.ToString(), tournamentname);
            }
        }

        public async Task ComputeScore25p(Participant p, string tournamentname)
        {
            if (Int32.Parse(p.final_rank.ToString()) == 1)
            {
                await DBEngine.AddTournamentPoints(10, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) == 2)
            {
                await DBEngine.AddTournamentPoints(7, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) == 3 || Int32.Parse(p.final_rank.ToString()) == 4)
            {
                await DBEngine.AddTournamentPoints(5, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) <= 8)
            {
                await DBEngine.AddTournamentPoints(3, p.username.ToString(), tournamentname);
            }
            else if (Int32.Parse(p.final_rank.ToString()) <= 16)
            {
                await DBEngine.AddTournamentPoints(2, p.username.ToString(), tournamentname);
            }
            else
            {
                await DBEngine.AddTournamentPoints(1, p.username.ToString(), tournamentname);
            }
        }

        private Participants.Root ReturnParticipants(string name, List<Participants.Root> Participants)
        {
            foreach (Participants.Root p in Participants)
            {
                if (p.participant.name == name)
                {
                    return p;
                }
            }
            return null;
        }

        private async Task SendDropDownList(List<DiscordSelectComponentOption> matchListOption, InteractionContext ctx)
        {
            IEnumerable<DiscordSelectComponentOption> options = matchListOption.AsEnumerable();

            var dropdown = new DiscordSelectComponent("scoreDropDownList", "Select...", options);

            var dropDownMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Azure)
                .WithTitle("Choisissez un match"))
                .AddComponents(dropdown);

            await ctx.Channel.SendMessageAsync(dropDownMessage);
        }
        /*
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        */
    }
}
