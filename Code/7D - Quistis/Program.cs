using _7D___Quistis.commands;
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

namespace _7D___Quistis
{
    internal class Program
    {
        //Instance of discord
        private static DiscordClient Client { get; set; }
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

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = Client.UseSlashCommands();

            slashCommandsConfig.RegisterCommands<SlashCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
