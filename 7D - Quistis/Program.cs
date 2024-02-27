using _7D___Quistis.SlashCommands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using APIChallongeClass;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using System;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using _7D___Quistis.DataBase;
using Npgsql;
using System.Xml.Linq;
using System.Reflection;
using DSharpPlus.AsyncEvents;
using DSharpPlus.SlashCommands.EventArgs;
using System.Threading;
using System.Windows.Forms;

namespace _7D___Quistis
{
    internal class Program
    {
        //Instance of discord
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        static async Task Main(string[] args)
        {
            //discord bot configuration
            var jsonReader = new JSONReaderClass("config.json");
            await jsonReader.ReadJSON();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };
            
            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += Client_Ready;
            Client.ComponentInteractionCreated += Button_Pressed;
            Client.ModalSubmitted += ModalEventHandler;
            
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = Client.UseSlashCommands();

            slashCommandsConfig.RegisterCommands<ChallongeCommands>();
            slashCommandsConfig.SlashCommandExecuted += CommandsExecuted;
            await Client.ConnectAsync();
            while (true)
            {
                await Task.Delay(-1);
            }
        }

        private static async Task CommandsExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
        {
            string option = "";
            string value = "";

			if (args.Context.Interaction.Data.Options != null)
            {
                foreach (var options in args.Context.Interaction.Data.Options)
                {
                    if (args.Context.Interaction.Data.Options.Last().Equals(options))
                    {
                        option += options.Name;
                        value += options.Value.ToString();
                    }
                    else
                    {
						option += options.Name + ", ";
						value += options.Value.ToString() + ", ";
					}
                };
            }
            await DBEngine.AddLogs(args.Context.CommandName, args.Context.Member.Nickname, option, value);
        }
        public static async Task Button_Pressed(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if(args.Interaction.Data.CustomId == "scoreDropDownList")
            {
                string selectedOption = args.Values.First().ToString();
                string[] values = selectedOption.Split(' ');
                if(values.Count() < 4)
                {
                    await args.Interaction.Channel.SendMessageAsync("une erreur c'est produite, svp contacter @nekoyuki");
                    throw new Exception("value from scoredropdownlist less than 4");
                }
                await ChallongeCommands.DisplayScoreModal(values[0], values[1], values[2], values[3], args);
            }
            else if (args.Interaction.Data.CustomId == "RegistrationButton")
            {
                await ConnectionChallongeAPI.AddParticipant(args.Message.Components.First().Components.First().CustomId, args.User.Username);

                //add participant to database
                bool addedToDB = await DBEngine.StorePlayer(args.User.Username);

                if (addedToDB)
                {
                    await args.Channel.SendMessageAsync($"{args.User.Username} a effectué sa première inscription !");
                }
                DSharpPlus.Entities.DiscordButtonComponent label = (DiscordButtonComponent)args.Message.Components.First().Components.First();
                
                //ask participant for info
                await ChallongeCommands.DisplayRegistrationModal(label.Label, args);
            }
        }

        private static async Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            await Task.CompletedTask;
        }

        public static async Task ModalEventHandler(DiscordClient sender, ModalSubmitEventArgs args)
        {
            if (args.Interaction.Data.CustomId == "Score")
            {
                var values = args.Values;

                //tournoi, match, j1, j2, winner
                await ConnectionChallongeAPI.AddScore(values.Values.ElementAt(0), values.Values.ElementAt(1), Int32.Parse(values.Values.ElementAt(2)), Int32.Parse(values.Values.ElementAt(3)), values.Values.ElementAt(4));
                //matchid, j1, j2, winner
                await DBEngine.AddScoreToMatch(Int32.Parse(values.Values.ElementAt(1)), Int32.Parse(values.Values.ElementAt(2)), Int32.Parse(values.Values.ElementAt(3)), values.Values.ElementAt(4));

                try
                {
                    string looser = values.Values.ElementAt(4) != args.Values.ElementAt(3).Key ? args.Values.ElementAt(3).Key : args.Values.ElementAt(2).Key;
                    //compute bounty
                    if (await DBEngine.GetScore(values.Values.ElementAt(4)) * 2 < await DBEngine.GetScore(looser))
                    {
                            await DBEngine.AddBounty(values.Values.ElementAt(4));
                    }
                }
                catch
                {
                    await args.Interaction.Channel.SendMessageAsync("Error Something went wrong please contact @Nekoyuki");
                }
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Le score a bien été ajouté, merci :) "));

            }
            else if (args.Interaction.Data.CustomId == "RegistrationTournament") 
            {
                var values = args.Values;
                await ConnectionChallongeAPI.AddParticipant(values.Values.ElementAt(0), args.Interaction.User.Username);
                await DBEngine.PostRegistration(values.Values.ElementAt(1),values.Values.ElementAt(0), args.Interaction.User.Username);
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{args.Interaction.User.Username} Merci pour votre inscription :) "));
            }
        }
    }
}
