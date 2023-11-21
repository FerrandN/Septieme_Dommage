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

            await Client.ConnectAsync();
            while (true)
            {
                await Task.Delay(-1);
            }
        }

        public static async Task Button_Pressed(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            var jsonReader = new JSONReaderSubdomainClass("subdomain.json");
            await jsonReader.ReadJSON();
            string name = args.User.Username;//name of participant
                                            //create element to send to API
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("participant[name]", name);
            if (jsonReader.subdomain != "")
            {
                dic.Add("{tournament}", jsonReader.subdomain + "-" + args.Message.Components.First().Components.First().CustomId);
            }
            else
            {
                dic.Add("{tournament}", args.Interaction.Data.Name);
            }
                //send to API
                await ConnectionChallongeAPI.AddParticipant(dic);

            try
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(args.User.Username + " Merci pour votre inscription :)"));
            }
            catch
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().WithContent("Une erreur c'est produite, merci de contact @Nekoyuki0070"));
            }

        }

        private static async Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            await Task.CompletedTask;
        }
    }
}
