using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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

        public void RegistryTelemetry(EventTelemetry et)
        {
            Telemetry.TrackEvent(et);
        }

        public void RegistryNewMessageSent(ITurnContext context)
        {
            if (context.Activity.Type != ActivityTypes.Message)
                return;

            var et = GetEventTelemetryForNewMessageFromUser();
            et.Properties.Add("SenderId", Utils.GetSenderIdFromContext(context));
            et.Properties.Add("MessageSent", context.Activity.Text);
            et.Properties.Add("ChannelId", context.Activity.ChannelId);
            et.Properties.Add("LocalTimestamp", context.Activity.LocalTimestamp.HasValue ? context.Activity.LocalTimestamp.Value.ToString() : DateTime.UtcNow.ToString());
            RegistryTelemetry(et);
        }

    }
}
