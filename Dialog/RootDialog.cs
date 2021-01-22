// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using QnABot.Classes;
using QnABot.Dialog;
using QnABot.Interfaces;
using QnABot.Luis;

namespace Microsoft.BotBuilderSamples.Dialog
{
    public class RootDialog : ComponentDialog
    {

        public RootDialog(IBotServices services, IConfiguration configuration, IWeather weather, TelemetryHandle telemetry)
            : base("root")
        {
            AddDefaultDialog();

            AddDialog(CreateCancelInterrupt(services, weather, configuration, telemetry));

            GetAllDialogs(services, weather, configuration, telemetry).ForEach(x =>
            {
                AddDialog(x);
            });

            InitialDialogId = nameof(AdaptiveDialog);
        }

        private List<Microsoft.Bot.Builder.Dialogs.Dialog> GetAllDialogs(IBotServices services, IWeather weather,
            IConfiguration configuration, TelemetryHandle telemetry)
        {
            List<Microsoft.Bot.Builder.Dialogs.Dialog> dialogs = new List<Bot.Builder.Dialogs.Dialog>();
            // QnAMakerBaseDialog
            dialogs.Add(new QnAMakerBaseDialog(services));

            // Travel booking
            var travelBookingDialog = new TravelBookingDialog();
            dialogs.Add(new WaterfallDialog(DialogName.HandleTravelBookingDialog, new WaterfallStep[]
                {
                    travelBookingDialog.GetCity,
                    travelBookingDialog.GetDate,
                    travelBookingDialog.GetNumberOfPerson,
                    travelBookingDialog.FinalStep
                }));

            // Booking restaurant
            var restaurantBookingDialog = new BookingRestaurantDialog();
            dialogs.Add(new WaterfallDialog(DialogName.HandleBookingRestaurant, new WaterfallStep[]
                {
                    restaurantBookingDialog.GetCity,
                    restaurantBookingDialog.GetDate,
                    restaurantBookingDialog.GetNumberOfPerson,
                    restaurantBookingDialog.FinalStep
                }));

            // Travel 
            var travelDialog = new TravelDialog();
            dialogs.Add(new WaterfallDialog(DialogName.HandleTravelDialog, new WaterfallStep[]
                {
                    travelDialog.GetRandomCity
                }));

            // Meteo
            var meteoDialog = new MeteoDialog(weather);
            dialogs.Add(new WaterfallDialog(DialogName.HandleMeteo, new WaterfallStep[]
                {
                    meteoDialog.GetCityFromUser,
                    meteoDialog.GetDataFromUser,
                    meteoDialog.GetMeteoFromService
                }));

            // Luis Dialog
            var luisDialog = new LuisDialog(configuration, telemetry);
            dialogs.Add(new WaterfallDialog(DialogName.LuisDialog, new WaterfallStep[]
                {
                    luisDialog.IntroStepAsync,
                    luisDialog.LuisStepAsync,
                    luisDialog.FinalStepAsync
                }
           ));

            return dialogs;
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

        private AdaptiveDialog CreateCancelInterrupt(IBotServices services, IWeather weather,
            IConfiguration configuration, TelemetryHandle telemetry)
        {
            var dialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                AutoEndDialog = false,
                Recognizer = CreateLuisRecognizer(configuration),
                Triggers = new List<OnCondition>() {
                    new OnIntent() {
                        Intent = "Weather",
                        Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "turn.recognized.score >= 0.8",
                                Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.HandleMeteo, "turn")
                                },
                                ElseActions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.QnAMakerDialog)
                                }
                            }
                        },
                    }, 
                    new OnIntent() {
                        Intent = "TravelBooking",
                        Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "turn.recognized.score >= 0.8",
                                Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.HandleTravelBookingDialog, "turn")
                                },
                                ElseActions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.QnAMakerDialog)
                                }
                            }
                        },
                    }, 
                    new OnIntent() {
                        Intent = "BookingRestaurant",
                        Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "turn.recognized.score >= 0.8",
                                Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.HandleBookingRestaurant, "turn")
                                },
                                ElseActions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.QnAMakerDialog)
                                }
                            }
                        },
                    },
                    new OnIntent() {
                        Intent = "TravelExample",
                        Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "turn.recognized.score >= 0.8",
                                Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.HandleTravelDialog, "turn")
                                },
                                ElseActions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                {
                                    new BeginDialog(DialogName.QnAMakerDialog)
                                }
                            }
                        },
                        Condition = "turn.recognized.score >= 0.8"
                    },
                    new OnIntent() {
                        Intent = "Cancel",
                        Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>() {
                                new ConfirmInput()
                                {
                                    Property = "turn.confirm",
                                    AllowInterruptions = false,
                                    Prompt = new ActivityTemplate("Confermi l'uscita?")
                                },
                                new IfCondition()
                                {
                                    Condition = "turn.confirm == true",
                                    Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                    {
                                        new SendActivity("Bene, ricominciamo!"),
                                        new CancelAllDialogs()
                                    },
                                    ElseActions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>()
                                    {
                                        new SendActivity("Bene, continuiamo!")
                                    }
                                }
                        },
                        Condition = "turn.recognized.score >= 0.8"
                    },
                    new OnIntent() {
                        Intent = "None",
                        Actions = new List<Microsoft.Bot.Builder.Dialogs.Dialog>() 
                        {
                            new ReplaceDialog(DialogName.QnAMakerDialog, null)
                        }
                    }
                }
            };
            return dialog;
        }


        private static Recognizer CreateLuisRecognizer(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["LuisAppId"]) || string.IsNullOrEmpty(configuration["LuisAPIKey"]) || string.IsNullOrEmpty(configuration["LuisAPIHostName"]))
            {
                throw new Exception("NOTE: LUIS is not configured for RootDialog. To enable all capabilities, add 'LuisAppId-RootDialog', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.");
            }

            return new LuisAdaptiveRecognizer()
            {
                ApplicationId = configuration["LuisAppId"],
                EndpointKey = configuration["LuisAPIKey"],
                Endpoint = configuration["LuisAPIHostName"]
            };
        }
    }
}
