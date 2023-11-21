using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace AutocompleteCommand
{
    internal class CommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("test", "test")]
        public async Task ExampleCommand(InteractionContext ctx,
            [Summary("parameter_name")]
            [Autocomplete(typeof(AutocompleteCommand.AutocompleteHandlers))] string tournament)
            => await Context.Interaction.RespondAsync($"Hello {tournament}", ephemeral: true);
    }
}
