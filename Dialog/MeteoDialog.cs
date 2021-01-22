using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Schema;
using QnABot.Classes;
using QnABot.Interfaces;
using QnABot.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QnABot.Dialog
{
    public class MeteoDialog
    {
        private readonly IWeather weather;

        public MeteoDialog(IWeather weather)
        {
            this.weather = weather;
        }

        public async Task<DialogTurnResult> GetCityFromUser(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            var resultFromLuis = new ResultFromLuis(context.Options);
            bool previusError = CheckPreviusError(context, "errorCity");
            //if previusError == true the old date is not valid and user must insert new city
            if (!previusError && resultFromLuis != null && Utils.ExistPropertyInEntitiesFromLuis(resultFromLuis, "Citta"))
            {
                var city = Utils.GetPropertyInEntitiesFromLuis<string>(resultFromLuis, "Citta");
                if (string.IsNullOrEmpty(city))
                    previusError = true;
                else
                {
                    context.Values["cityForMeteo"] = city;
                    return await context.NextAsync();
                }
            }

            var messageText = previusError ?
                                $"La città {context.Values["cityForMeteo"]} non è risultata valida. Inseriscine un'altra:" :
                                $"Di quale città vuoi sapere le previsioni del tempo?";

            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            PromptOptions promptOptions = new PromptOptions
            {
                Prompt = message
            };

            return await context.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        public async Task<DialogTurnResult> GetDataFromUser(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            ChooseRightCityForContext(context);
            context.Values.TryGetValue("errorDatetime", out object obj);
            bool previusError = obj != null ? (bool)obj : false;

            var resultFromLuis = new ResultFromLuis(context.Options);

            //if previusError == true the old date is not valid and user must insert new dateù
            if (!previusError && resultFromLuis != null && Utils.ExistPropertyInEntitiesFromLuis(resultFromLuis, "datetime"))
            {
                // save date and go away if it is ok
                var date = Utils.GetPropertyInEntitiesFromLuis<DateTime>(resultFromLuis, "datetime");
                if (date == null || DateTime.Now > date || DateTime.Now.AddDays(5) < date)
                {
                    previusError = true;
                }
                else
                {
                    context.Values["dateForMeteo"] = date;
                    return await context.NextAsync();
                }
            }

            var messageText = previusError ?
                                $"La data è troppo lontana oppure non è valida. Inseriscine una più vicina." :
                                $"Previsioni di quando?";
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            var retryMessageText = "La data inserita non sembra valida. Prova ad utilizzare il formato MM/GG/AAAA";
            var retryMessage = MessageFactory.Text(retryMessageText, retryMessageText, InputHints.IgnoringInput);
            PromptOptions promptOptions = new PromptOptions
            {
                Prompt = message,
                RetryPrompt = retryMessage
            };

            return await context.PromptAsync(nameof(DateTimePrompt), promptOptions, cancellationToken);
        }

        public async Task<DialogTurnResult> GetMeteoFromService(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            ChooseRightDataForContext(context);

            var city = (string)context.Values["cityForMeteo"];
            var date = (DateTime)context.Values["dateForMeteo"];
            var weatherResult = await weather.GetWeather(city);

            if (weatherResult.Cod != "200")
            {
                context.Values["errorCity"] = true;
                return await context.ReplaceDialogAsync(DialogName.HandleMeteo, context.Options, cancellationToken);
            }
            else
            {
                var weather = weatherResult.GetWeatherForDate(date);
                var cardAttachment = Utils.CreateAdaptiveCardAttachmentFromWeatherObj(weather, city);

                await context.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                await context.EndDialogAsync();
                return await context.BeginDialogAsync(nameof(AdaptiveDialog), null);
            }            
        }

        private void ChooseRightDataForContext(WaterfallStepContext context)
        {
            if (context.Result == null)
                return;
            var tempDateTime = ((List<DateTimeResolution>)context.Result).FirstOrDefault()?.Value; // TODO: sistemare l'estrapolazione della data dall'oggetto complicato
            var date = tempDateTime != null ? DateTime.Parse(tempDateTime) : (DateTime)context.Values["dateForMeteo"];
            context.Values["dateForMeteo"] = date;
        }

        private void ChooseRightCityForContext(WaterfallStepContext context)
        {
            if (context.Result == null)
                return;
            var cityFromContext = (string)context.Result;
            if (!string.IsNullOrEmpty(cityFromContext))
                context.Values["cityForMeteo"] = cityFromContext;
        }

        private bool CheckPreviusError(WaterfallStepContext context, string errorName)
        {
            bool previusError = false;
            if (context.Stack.Count > 2 && context.Stack[1].Id == DialogName.HandleMeteo.ToString())
            {
                // Check error in the previusly context state
                context.Stack[1].State.TryGetValue("values", out object oldValues);
                Dictionary<string, object> oldDictionary = oldValues != null ? 
                                                            (Dictionary<string, object>)oldValues :
                                                            null;
                oldDictionary.TryGetValue(errorName, out object oldError);

                previusError = oldError != null ? (bool)oldError : false;
            }

            return previusError;
        }
    }
}
