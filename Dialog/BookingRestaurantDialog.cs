using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using QnABot.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QnABot.Dialog
{
    public class BookingRestaurantDialog
    {
        public async Task<DialogTurnResult> GetCity(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            var message2 = "Dove vuoi andare a mangiare:";
            Activity textPrompt = context.Context.Activity.CreateReply(message2);
            Activity retryPrompt = context.Context.Activity.CreateReply("Scusami, la tua risposta non è tra le scelte possibili, riprova.");
            PromptOptions promptOptions = new PromptOptions
            {
                Prompt = textPrompt,
                Choices = new List<Choice> { 
                    new Choice("Milano"), 
                    new Choice("Torino"), 
                    new Choice("Roma"), 
                    new Choice("Catania"), 
                    new Choice("Genova"), 
                    new Choice("Londra"), 
                    new Choice("New York") 
                },
                RetryPrompt = retryPrompt,
                Style = ListStyle.HeroCard
            };
            
            return await context.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        public async Task<DialogTurnResult> GetDate(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            context.Values["destination"] = ((FoundChoice)context.Result).Value;

            Activity activity = context.Context.Activity.CreateReply("Quando vuoi uscire?");
            Activity retryActivity = context.Context.Activity.CreateReply("Scusami ma non ho capito. Inserisci la data come MM/GG/AAAA");

            PromptOptions promptOptions = new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = retryActivity
            };

            return await context.PromptAsync(nameof(DateTimePrompt), promptOptions, cancellationToken);
        }

        public async Task<DialogTurnResult> GetNumberOfPerson(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            context.Values["departourDate"] = ((List<DateTimeResolution>)context.Result).FirstOrDefault()?.Value;

            Activity activity = context.Context.Activity.CreateReply("In quanti siete?");
            Activity retry = context.Context.Activity.CreateReply("Scusami ma non hai inserito un numero, ritenta.");
            PromptOptions promptOptions = new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = retry
            };

            return await context.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }
        public async Task<DialogTurnResult> FinalStep(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            int numberOfPerson = (int)context.Result;
            string destination = context.Values["destination"].ToString();
            string date = context.Values["departourDate"].ToString();

            var messageText = $"Bene, è stato prenotato un tavolo per {numberOfPerson} persone. Il tavolo sarà a {destination} il {date}.";
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await context.Context.SendActivityAsync(message, cancellationToken);
            await context.EndDialogAsync();
            return await context.BeginDialogAsync(DialogName.LuisDialog, null, cancellationToken);
        }
    }
}
