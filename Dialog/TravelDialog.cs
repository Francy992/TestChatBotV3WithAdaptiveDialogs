using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using QnABot.Classes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QnABot.Dialog
{
    public class TravelDialog
    {
        public async Task<DialogTurnResult> GetRandomCity(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            //var temp = JsonConvert.DeserializeObject<ResultFromLuis>((string)context.Values[DialogName.ResultFromLuis]);
            var messageText = $"Ti consiglio di andare a ...";
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await context.Context.SendActivityAsync(message, cancellationToken);
            var rand = new Random();

            messageText = rand.Next(100) > 50 ? "Roma" : "Milano";
            message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await context.Context.SendActivityAsync(message, cancellationToken);

            messageText = "In che altro modo posso esserti utile?";
            message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await context.Context.SendActivityAsync(message, cancellationToken);
            await context.EndDialogAsync();
            return await context.BeginDialogAsync(DialogName.LuisDialog, null, cancellationToken);
        }
    }
}
