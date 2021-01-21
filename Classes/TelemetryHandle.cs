using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using QnABot.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QnABot.Classes
{
    public class TelemetryHandle
    {
        private readonly TelemetryClient Telemetry;

        public TelemetryHandle(IConfiguration appSettings)
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = appSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
            Telemetry = new TelemetryClient(configuration);
        }

        public EventTelemetry GetEventTelemetryForStartConversation()
        {
            return new EventTelemetry("StartConversation");
        }

        public EventTelemetry GetEventTelemetryForNewMessageFromUser()
        {
            return new EventTelemetry("NewMessageFromUser");
        }

        public EventTelemetry GetEventTelemetryForLuisNotResult()
        {
            return new EventTelemetry("LuisResult");
        }
        
        public EventTelemetry GetEventTelemetryNotHandleException()
        {
            return new EventTelemetry("NotHandleException");
        }

        public void RegistryNewUserAdd(string memberId)
        {
            var et = GetEventTelemetryForNewMessageFromUser();
            et.Properties.Add("MemberId", memberId);

            RegistryTelemetry(et);
        }

        public void RegistryNewMessageSent(ITurnContext context)
        {
            if (context.Activity.Type != ActivityTypes.Message)
                return;

            var et = GetEventTelemetryForNewMessageFromUser();
            et.Properties.Add("SenderId", Utils.GetSenderIdFromContext(context));
            et.Properties.Add("MessageSent", context.Activity.Text);
            et.Properties.Add("ChannelId", context.Activity.ChannelId);

            RegistryTelemetry(et);
        }

        public void RegistryLuisResult(ResultFromLuis luisResult, WaterfallStepContext context)
        {
            if (context.Context.Activity.Type != ActivityTypes.Message)
                return;

            var et = GetEventTelemetryForLuisNotResult();
            et.Properties.Add("SenderId", Utils.GetSenderIdFromContext(context.Context));
            et.Properties.Add("MessageSent", context.Context.Activity.Text);
            et.Properties.Add("ChannelId", context.Context.Activity.ChannelId);
            et.Properties.Add("LuisScore", luisResult.GetScore().ToString());
            et.Properties.Add("LuisIntent", luisResult.GetIntent());

            RegistryTelemetry(et);
        }

        public void RegistryNotHandleException(ITurnContext context, Exception e)
        {
            var et = GetEventTelemetryNotHandleException();
            et.Properties.Add("SenderId", Utils.GetSenderIdFromContext(context));
            et.Properties.Add("MessageSent", context.Activity.Text);
            et.Properties.Add("ChannelId", context.Activity.ChannelId);
            et.Properties.Add("ExceptionMessage", e.Message);
            et.Properties.Add("ExceptionStackTrace", e.StackTrace);
            et.Properties.Add("InnerException", e.InnerException?.ToString());
            et.Properties.Add("LocalTimestamp", DateTime.UtcNow.ToString());
            Telemetry.TrackException(e, et.Properties);
        }

        public void RegistryTelemetry(EventTelemetry et)
        {
            et.Properties.Add("LocalTimestamp", DateTime.UtcNow.ToString());
            Telemetry.TrackEvent(et);
        }

    }
}
