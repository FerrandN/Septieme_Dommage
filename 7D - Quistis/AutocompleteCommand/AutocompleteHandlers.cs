using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocompleteCommand
{
    public class AutocompleteHandlers : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(new[] { "Huey", "Dewey", "Louie" }.Select(s => new AutocompleteResult(s, s))));
        
    }
}
