using APIChallongeClass;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using static APIChallongeClass.Participants;
using _7D___Quistis.ChoiceProvider;
using DSharpPlus.EventArgs;
using _7D___Quistis.DataBase;
using System.Net.Http;
using JWTManager;
using System.Net.Http.Headers;
using System.Xml.Schema;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using System.Text.RegularExpressions;

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
        [Option("DateDeDepart", "format: YYYY-MM-DD HH:MM:SS")] string date, //date and hours at which tournament starts

        [Option("IdDuTournoi", "l'identifiant Du Tournoi")] string url) //tournament id) 
        {
            if (await HasPermission(ctx) && TestDateTime(date))
            {
                //tell discord to wait for response
                await ctx.DeferAsync();

                //create element to send to API
                Dictionary<string, string> dic = new Dictionary<string, string>();
				name = RemoveSpecialCharacters(name);
				url.Replace(" ", "_");
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
                    DateTime time = DateTime.Parse(date);
                    //add embed
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Creer Tournoi"
                    }));

                    //store data in DB
                    await DBEngine.PostTournament(type.Replace(' ', '_'), name, time, url);


                    //display info if everything is ok
                    DiscordMessageBuilder embedMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Worked perfectly")
                        .WithDescription("Tournament created at: https://challonge.com/fr/" + url));

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament created at: https://challonge.com/fr/" + url, true));
                    DiscordMessageBuilder messageWithButton = new DiscordMessageBuilder();

                    messageWithButton.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "RegistrationButton", $"{url}"));
                    DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(date);
                    await ctx.Guild.CreateEventAsync("Tournoi: " + name, type, null, ScheduledGuildEventType.External, ScheduledGuildEventPrivacyLevel.GuildOnly, dateTimeOffset, dateTimeOffset.AddHours(5), "Discord 7D");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder(messageWithButton));

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync(SetMessage("Error", "Wrong date format, must be later than today and written as following example: 2052-12-24 18:00:00", false));
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

                    await ctx.Channel.SendMessageAsync(SetMessage("Worked perfectly", "Tournament as started !! GL (Monks are the bests)", true));

					await DisplayMatches(ctx, tournamenturl);
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
                    await DBEngine.PostEndDate(tournamenturl);
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
            await ctx.DeferAsync(true);

            string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
            string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
            string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

            List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

            List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

            TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

            List<MatchesData.Root> playerMatches = new List<MatchesData.Root>();

            List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();

            //Ajouter les matches du joueur dans la liste "matche"
            if(tournament.tournament.tournament_type != "round robin")
            {
                matchListOption = await SetMatcheListWithParticipants(playerMatches,participants,matches,ctx.User.Username,tournamenturl);
			}
            else
            {
				matchListOption = await SetMatcheListWithParticipantsRR(playerMatches, participants, matches, ctx.User.Username, tournamenturl);
			}
            await SendDropDownList(matchListOption, ctx);
		}

		[SlashCommand("ChangeScore", "Change un score du tournoi")]
		public async Task ChangeScore(InteractionContext ctx,
		[ChoiceProvider(typeof(DiscordChoiceProviderGetAllMatches))]
		[Option("match","Match Id")] string matchId,

		[ChoiceProvider(typeof(DiscordChoiceProviderGetInProgressTournament))]
		[Option("tournament","Tournament URL")] string tournamenturl,

		[Option("winner", "Nom du gagnant")]DiscordUser user,
        [Option("ScoreJ1", "ScoreJ1")]long scoreJ1,
	    [Option("ScoreJ2", "ScoreJ2")]long scoreJ2)
		{
            if(!await HasPermission(ctx))
            { return; }
            int id = 0;
			await ctx.DeferAsync();
            try
            {
                id = Int32.Parse(matchId);
            }
            catch
            {
				await ctx.Channel.SendMessageAsync
                ($"ID Incorecte");
                return;
			}
            await ConnectionChallongeAPI.AddScore(tournamenturl,matchId, (int)scoreJ1, (int)scoreJ2,user.Username);
            await DBEngine.ChangeMatchesScore(id,(int)scoreJ1,(int)scoreJ2,user.Username);
			await ctx.Channel.SendMessageAsync
	        ($"Le matche {matchId} a été modifié. Le gagnant est {user.Username} avec un score de: J1: {scoreJ1} J2: {scoreJ2}");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder());
		}

		[SlashCommand("ToutScores", "Ajoute le score d'un joueur au tournoi")]
		public async Task AddOthersScore(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetInProgressTournament))]
		[Option("tournament","Tournament URL")] string tournamenturl)
		{
            if (!await HasPermission(ctx))
            {
				return;
            }
			await ctx.DeferAsync(true);

			string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
			string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
			string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

			List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

			List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

			TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

			List<MatchesData.Root> playerMatches = new List<MatchesData.Root>();

			List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();

			//Ajouter les matches du joueur dans la liste "matche"
			if (tournament.tournament.tournament_type != "round robin")
			{
				matchListOption = await SetAllMatcheListWithParticipants(playerMatches, participants, matches, ctx.User.Username, tournamenturl);
			}
			else
			{
				matchListOption = await SetAllMatcheListWithParticipantsRR(playerMatches, participants, matches, ctx.User.Username, tournamenturl);
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

                string result = await ConnectionChallongeAPI.GetTournamentWithState("","pending");

                List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

				try
				{
					    //For every tournaments found, we add a button with tournament Name.
					    foreach (TournamentsData.Root tournament in tournaments)
                    {
					    DiscordMessageBuilder messageWithButton = new DiscordMessageBuilder();
                        messageWithButton.Content = tournament.tournament.name.ToString();
					    messageWithButton.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "RegistrationButton", tournament.tournament.url.ToString()));
                        await ctx.Channel.SendMessageAsync(messageWithButton);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
					DiscordMessageBuilder message = new DiscordMessageBuilder();
                    message.Content = "Appuyer sur un bouton pour vous inscrire";
					await ctx.EditResponseAsync(new DiscordWebhookBuilder(message));
                }
                catch (Exception ex)
                {
					Console.WriteLine(ex.ToString());
                }
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
			await ctx.DeferAsync();
			await Program.Client.GetSlashCommands().RefreshCommands();
			await ctx.EditResponseAsync(new DiscordWebhookBuilder()
	        .AddEmbed(new DiscordEmbedBuilder()
	        {
		        Title = "Refresh terminé"
	        }));
		}

        [SlashCommand("ChangePointJoueur", "change Point")]
        public async Task ChangePlayerPoint(InteractionContext ctx,
            [Option("points","points")] string userInput,
            [Option("joueur","joueur")] DiscordUser user)
        {
            if(await HasPermission(ctx))
            {
                int points = Int32.Parse(userInput);
                await DBEngine.ChangePlayerPoints(points, user.Username);
                if (points > 0)
                {
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
	                    Title = $"l'utilisateur {user.Username} a gagné {points}"
                    }.WithColor(DiscordColor.Blue)));
				}
                else
                {
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
	                    Title = $"{points} ont été retiré a l'utilisateur {user.Username}, villain !"
                    }.WithColor(DiscordColor.Red)));
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
                await DBEngine.ChangePlayerNameAndUsername(surname, name, user.Username);
                DiscordMessageBuilder message = new DiscordMessageBuilder();
                message.Content = $"l'utilisateur {user.Username} a correctement été nommé {surname} {name}";
				await ctx.EditResponseAsync(new DiscordWebhookBuilder(message));
            }

        }

        [SlashCommand("AjouteMatchesSite", "Ajoute tout les Matches d'un tournoi terminé au site")]
        public async Task AddWebSiteMatches(InteractionContext ctx,
		[ChoiceProvider(typeof(DiscordChoiceProviderGetEndedTournament))]
		[Option("tournament","Tournament URL")] string tournamenturl)
		{ 
            await ctx.DeferAsync();

			if (await HasPermission(ctx))
			{
				try
				{
					string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
					string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
					string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

					List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

					List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

					TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

					Dictionary<string, string> participantsName = new Dictionary<string, string>();

					if (tournament.tournament.tournament_type != "round robin")
					{

						foreach (MatchesData.Root match in matches)
						{
							string player1Name = "";
							string player2Name = "";

							foreach (Participants.Root participant in participants)
							{
								if (participant.participant.id == match.match.player1_id)
								{
									player1Name = participant.participant.name;
								}
								if (participant.participant.id == match.match.player2_id)
								{
									player2Name = participant.participant.name;
								}
							}
							await DBEngine.PostMatch(match.match.id,tournamenturl, player1Name, player2Name);
						}
					}
					else
					{
						foreach (MatchesData.Root match in matches)
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
							await DBEngine.PostMatch(match.match.id, tournamenturl, player1Name, player2Name);
						}
					}
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
	                    Title = "Tâche accomplie"
                    }));
				}
				catch
				{
					await ctx.Channel.SendMessageAsync(SetMessage("Error", "Something went wrong please retry later or contact @Nekoyuki", false));
				}
			}

		}

        [SlashCommand("Test", "Test")]
        [RequirePermissions(Permissions.Administrator, true)]
        public async Task Test(InteractionContext ctx)
        {
			string result = await ConnectionChallongeAPI.GetTournament("");

			List<TournamentsData.Root> tournaments = JsonConvert.DeserializeObject<List<TournamentsData.Root>>(result);

            foreach (TournamentsData.Root tournament in tournaments)
            {
                await DBEngine.UpdateAllUrl(tournament.tournament.name,tournament.tournament.url);
                Console.WriteLine($"{tournament.tournament.name}, {tournament.tournament.url}");
				Thread.Sleep(5000);
			}
		}

        [SlashCommand("Test2", "Test2")]
        [RequirePermissions(Permissions.Administrator, true)]
        public async Task Test2(InteractionContext ctx)
        {
            //Token Request
            string requestBody = "{\"grant_type\":\"authorization_code\",\"code\":\"39467\", \"client_id\":\"a192729b4fdd3d07ae9b4760f480cf14b50ad4d88cfe661adf39c2ecba1184ac\" }";
            string jwt = CreateJWT.GenerateJWT(requestBody);

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

        [SlashCommand("FusionneJoueur", "Fusionne les informations de 2 joueurs dans la base de données")]
        public async Task MergePlayer(InteractionContext ctx,
			[Option("JoueurASauvegarder", "Joueur à sauvegarder")] DiscordUser user,
			[Option("JoueurASupprimer", "Joueur à supprimer")] string nametodelete)
        {
            if (!await HasPermission(ctx))
            {
                return;
            }
            await DBEngine.Merge2Players(user.Username, nametodelete);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder());
        }

		[SlashCommand("ChangeBonusJoueur", "change Point")]
		public async Task ChangePlayerBonus(InteractionContext ctx,
			[Option("points", "points")] string userInput,
			[Option("joueur", "joueur")] DiscordUser user)
		{
			if (await HasPermission(ctx))
			{
				int points = Int32.Parse(userInput);
				await DBEngine.ChangePlayerBonus(points, user.Username);
				if (points > 0)
				{
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
	                    Title = $"l'utilisateur {user.Username} a gagné {points}"
                    }.WithColor(DiscordColor.Blue)));
				}
				else
				{
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
	                    Title = $"{points} ont été retiré a l'utilisateur {user.Username}, villain !"
                    }.WithColor(DiscordColor.Red)));
				}
			}

		}

        [SlashCommand("Affiche_DeckList", "Affiche les deckList d'un tournoi")]
        public async Task DisplayDeckLists(InteractionContext ctx,
        [ChoiceProvider(typeof(DiscordChoiceProviderGetAllTournament))]
        [Option("tournament","Tournament URL")] string tournamenturl)
        {
            if (await HasPermission(ctx))
            {
				await ctx.DeferAsync();
				List<(string decklist, string playerNickname)> results = new List<(string decklist, string playerNickname)>();
				results = await DBEngine.GetDeckList(tournamenturl);

				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
	                Title = $"DeckLists du tournoi: {tournamenturl}"
                }));

				foreach (var result in results)
                {
                    await ctx.Channel.SendMessageAsync($"Joueur: {result.playerNickname}, DeckList: {result.decklist}");
                }
            }
        }

		[SlashCommand("ScoreJoueur", "Ajoute un score au tournoi")]
		public async Task AddPlayerScore(InteractionContext ctx,
	   [ChoiceProvider(typeof(DiscordChoiceProviderGetInProgressTournament))]
	    [Option("tournament","Tournament URL")] string tournamenturl,
        [Option("User","Nom du joueur")] DiscordUser user)
		{
            if (await HasPermission(ctx))
            {
                await ctx.DeferAsync(true);

                string result = await ConnectionChallongeAPI.GetMatches(tournamenturl);
                string participantsJson = await ConnectionChallongeAPI.GetParticipant(tournamenturl);
                string tournamentJson = await ConnectionChallongeAPI.GetTournament(tournamenturl);

                List<MatchesData.Root> matches = JsonConvert.DeserializeObject<List<MatchesData.Root>>(result);

                List<Participants.Root> participants = JsonConvert.DeserializeObject<List<Participants.Root>>(participantsJson);

                TournamentData.Root tournament = JsonConvert.DeserializeObject<TournamentData.Root>(tournamentJson);

                List<MatchesData.Root> playerMatches = new List<MatchesData.Root>();

                List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();

                //Ajouter les matches du joueur dans la liste "matche"
                if (tournament.tournament.tournament_type != "round robin")
                {
                    matchListOption = await SetMatcheListWithParticipants(playerMatches, participants, matches, user.Username, tournamenturl);
                }
                else
                {
                    matchListOption = await SetMatcheListWithParticipantsRR(playerMatches, participants, matches, user.Username, tournamenturl);
                }
                await SendDropDownList(matchListOption, ctx);
            }
		}
		public static async Task DisplayRegistrationModal(string tournamentName, ComponentInteractionCreateEventArgs e)
        {
            var modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Enregistrement tournoi")
            .WithCustomId("RegistrationTournament")
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

        private bool TestDateTime(string date)
        {
            DateTime dt = new DateTime();
            try
            {
                dt = DateTime.Parse(date);
            }
            catch
            {
                Console.WriteLine("datetime format error");
				return false;
			}

            if (DateTime.Now > DateTime.Parse(date))
            {
                return false;
            }
            else
            {
                return true;
            }
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
			await ctx.EditResponseAsync(new DiscordWebhookBuilder(dropDownMessage));
		}

        private async Task<List<DiscordSelectComponentOption>> SetMatcheListWithParticipants( List<MatchesData.Root> playerMatches, List<Participants.Root> participants, List<MatchesData.Root> matches, string userName, string tournamenturl)
        {
            List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();
			//on récupère les infos du joueur
			Participants.Root player = ReturnParticipants(userName, participants);
            if (player != null)
            {
				foreach (MatchesData.Root match in matches)
				{
					if (match.match.state != "complete")
					{
						if (match.match.player1_id == player.participant.id || match.match.player2_id == player.participant.id)
						{
							playerMatches.Add(match);
						}
					}
				}

				foreach (MatchesData.Root match in playerMatches)
				{
					string player1Name = "";
					string player2Name = "";

					foreach (Participants.Root participant in participants)
					{
						if (participant.participant.id == match.match.player1_id)
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
			return matchListOption;
		}

        private async Task<List<DiscordSelectComponentOption>> SetMatcheListWithParticipantsRR(List<MatchesData.Root> playerMatches, List<Participants.Root> participants, List<MatchesData.Root> matches, string userName, string tournamenturl)
        {
			List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();

			//on récupère les infos du joueur
			Participants.Root player = ReturnParticipants(userName, participants);

			foreach (MatchesData.Root match in matches)
			{
				if (match.match.player1_id.ToString() == player.participant.group_player_ids.First().ToString() || match.match.player2_id.ToString() == player.participant.group_player_ids.First().ToString())
				{
					playerMatches.Add(match);
				}
			}

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

			return matchListOption;
		}

		private async Task<List<DiscordSelectComponentOption>> SetAllMatcheListWithParticipants(List<MatchesData.Root> playerMatches, List<Participants.Root> participants, List<MatchesData.Root> matches, string userName, string tournamenturl)
		{
			List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();
			//on récupère les infos du joueur

			foreach (MatchesData.Root match in matches)
			{
				if (match.match.state != "complete")
				{
			        playerMatches.Add(match);
				}
			}

			foreach (MatchesData.Root match in playerMatches)
			{
				string player1Name = "";
				string player2Name = "";

				foreach (Participants.Root participant in participants)
				{
					if (participant.participant.id == match.match.player1_id)
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
			return matchListOption;
		}

		private async Task<List<DiscordSelectComponentOption>> SetAllMatcheListWithParticipantsRR(List<MatchesData.Root> playerMatches, List<Participants.Root> participants, List<MatchesData.Root> matches, string userName, string tournamenturl)
		{
			List<DiscordSelectComponentOption> matchListOption = new List<DiscordSelectComponentOption>();

			//on récupère les infos du joueur

			foreach (MatchesData.Root match in matches)
			{
			    playerMatches.Add(match);
			}

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

			return matchListOption;
		}

		private static string RemoveSpecialCharacters(string str)
		{

			Regex regex = new Regex("[^a-zA-Z0-9]");

			return regex.Replace(str, "");
		}
		/*
		[RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
		[RequireOwner(Group = "Permission")]
		*/
	}
}
