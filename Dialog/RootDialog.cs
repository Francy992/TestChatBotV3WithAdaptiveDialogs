// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using QnABot.Classes;
using QnABot.Dialog;
using QnABot.Interfaces;

namespace Microsoft.BotBuilderSamples.Dialog
{
    public class RootDialog : ComponentDialog
    {

        public RootDialog(IBotServices services, IConfiguration configuration, IWeather weather)
            : base("root")
        {
            AddDefaultDialog();

            // QnAMakerBaseDialog
            AddDialog(new QnAMakerBaseDialog(services));

            // Travel booking
            var travelBookingDialog = new TravelBookingDialog();
            AddDialog(new WaterfallDialog(DialogName.HandleTravelBookingDialog, new WaterfallStep[]
                {
                    travelBookingDialog.GetCity,
                    travelBookingDialog.GetDate,
                    travelBookingDialog.GetNumberOfPerson,
                    travelBookingDialog.FinalStep
                }));

            // Booking restaurant
            var restaurantBookingDialog = new BookingRestaurantDialog();
            AddDialog(new WaterfallDialog(DialogName.HandleBookingRestaurant, new WaterfallStep[]
                {
                    restaurantBookingDialog.GetCity,
                    restaurantBookingDialog.GetDate,
                    restaurantBookingDialog.GetNumberOfPerson,
                    restaurantBookingDialog.FinalStep
                }));

            // Travel 
            var travelDialog = new TravelDialog();
            AddDialog(new WaterfallDialog(DialogName.HandleTravelDialog, new WaterfallStep[]
                {
                    travelDialog.GetRandomCity
                }));

            // Meteo
            var meteoDialog = new MeteoDialog(weather);
            AddDialog(new WaterfallDialog(DialogName.HandleMeteo, new WaterfallStep[]
                {
                    meteoDialog.GetCityFromUser,
                    meteoDialog.GetDataFromUser,
                    meteoDialog.GetMeteoFromService
                }));

            // Luis Dialog
            var luisDialog = new LuisDialog(configuration);
            AddDialog(new WaterfallDialog(DialogName.LuisDialog, new WaterfallStep[] 
                {
                    luisDialog.IntroStepAsync,
                    luisDialog.LuisStepAsync,
                    luisDialog.FinalStepAsync
                }
           ));

            InitialDialogId = DialogName.LuisDialog;
        }

        private void AddDefaultDialog()
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));
        }

        // Example to Validator
        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }

        // Example to Validator
        private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var attachments = promptContext.Recognized.Value;
                var validImages = new List<Attachment>();

                foreach (var attachment in attachments)
                {
                    if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
                    {
                        validImages.Add(attachment);
                    }
                }

                promptContext.Recognized.Value = validImages;

                // If none of the attachments are valid images, the retry prompt should be sent.
                return validImages.Any();
            }
            else
            {
                await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a profile picture...");

                // We can return true from a validator function even if Recognized.Succeeded is false.
                return true;
            }
        }

    }
}
