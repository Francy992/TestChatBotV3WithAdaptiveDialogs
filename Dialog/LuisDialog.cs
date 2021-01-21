using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using QnABot.Classes;
using QnABot.Luis;
using System.Threading;
using System.Threading.Tasks;

namespace QnABot.Dialog
{
    public class LuisDialog
    {
        private readonly ConcreteLuisRecognizer _luisRecognizer;
        private ResultFromLuis _resultFromLuis;
        private IConfiguration configuration;

        public LuisDialog(IConfiguration configuration)
        {
            _luisRecognizer = new ConcreteLuisRecognizer(configuration);
            this.configuration = configuration;
        }

        public async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var messageText = "Ecco cosa so fare:";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            await stepContext.Context.SendActivityAsync(promptMessage);
            
            messageText = "Prenotare un ristorante, ricevere indicazioni di viaggio con \"Voglio partire\", prenotare un volo, sapere il meteo.";
            promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        public async Task<DialogTurnResult> LuisStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(WaterfallDialog), null, cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            _resultFromLuis = await _luisRecognizer.RecognizeAsync<ResultFromLuis>(stepContext.Context, cancellationToken);

            switch (_resultFromLuis.Intent)
            {
                case Intent.IdeeViaggio:
                    return await stepContext.BeginDialogAsync(DialogName.HandleTravelDialog, _resultFromLuis, cancellationToken);

                case Intent.PrenotazioneVolo: 
                    return await stepContext.BeginDialogAsync(DialogName.HandleTravelBookingDialog, _resultFromLuis, cancellationToken);
                    
                case Intent.Meteo: 
                    return await stepContext.BeginDialogAsync(DialogName.HandleMeteo, _resultFromLuis, cancellationToken);
                    
                case Intent.PrenotazioneRistorante: 
                    return await stepContext.BeginDialogAsync(DialogName.HandleBookingRestaurant, _resultFromLuis, cancellationToken);

                case Intent.None:
                default:
                    await stepContext.BeginDialogAsync(DialogName.QnAMakerDialog, _resultFromLuis, cancellationToken);
                    var promptMessage = "Posso fare altro per te?";
                    await stepContext.Context.SendActivityAsync(promptMessage);
                    return await stepContext.ReplaceDialogAsync(DialogName.LuisDialog, promptMessage, cancellationToken);
            }
        }

        public async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(null, cancellationToken);
                var promptMessage = "Posso fare altro per te?";
                return await stepContext.ReplaceDialogAsync(DialogName.LuisDialog, promptMessage, cancellationToken);
            
        }
    }
}
