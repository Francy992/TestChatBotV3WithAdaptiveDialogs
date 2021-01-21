using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using QnABot.Luis;
using QnABot.Model;
using System;
using System.IO;
using System.Linq;

namespace QnABot.Classes
{
    public static class Utils
    {
        public static bool ExistPropertyInEntitiesFromLuis(ResultFromLuis luisResult, string property)
        {
            bool found = false;
            try
            {
                luisResult.ResultObj.Entities.Root.ToList().ForEach(obj =>
                {
                    if (obj.Path.Equals(property))
                    {
                        found = true;
                    }
                });
            }
            catch { }

            return found;
        }

        public static T GetPropertyInEntitiesFromLuis<T>(ResultFromLuis luisResult, string property)
        {
            T city = default(T);
            try
            {
                if(typeof(T) == typeof(DateTime))
                {
                    var father = luisResult.ResultObj.Entities.Root.SelectToken(property);
                    return father.FirstOrDefault().SelectToken("timex").Values<T>().FirstOrDefault();;
                }
            
                return luisResult.ResultObj.Entities.Root.SelectToken(property).Values<T>().FirstOrDefault();
            }
            catch { }        

            return city;
        }
        
        public static Attachment CreateAdaptiveCardAttachment(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(".", "AdaptiveCardResources", "WeatherCard.json");

            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateAdaptiveCardAttachmentFromWeatherObj(WeatherListModel weather, string city)
        {
            var filePath = Path.Combine(".", "AdaptiveCardResources", "WeatherCard.json");
            string pictureLink = $"http://openweathermap.org/img/wn/{weather.Weather.FirstOrDefault()?.Icon}@4x.png";

            var adaptiveCardJson = File.ReadAllText(filePath);
            adaptiveCardJson = adaptiveCardJson.Replace("{-citta-}", city);
            adaptiveCardJson = adaptiveCardJson.Replace("{-data-}", weather.Dt_txt);
            adaptiveCardJson = adaptiveCardJson.Replace("{-temperatura-}", weather.Main.ToString());
            adaptiveCardJson = adaptiveCardJson.Replace("{-pressione-}", weather.Weather.FirstOrDefault()?.ToString() ?? "");
            adaptiveCardJson = adaptiveCardJson.Replace("{-vento-}", weather.Wind.ToString());
            adaptiveCardJson = adaptiveCardJson.Replace("{-picture-}", pictureLink);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static string GetSenderIdFromContext(ITurnContext context)
        {
            return context.Activity.From.Id;
        }
    }
}
