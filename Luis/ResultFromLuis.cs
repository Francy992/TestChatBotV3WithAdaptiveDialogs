using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.Json;

namespace QnABot.Luis
{
    // Extends the partial FlightBooking class with methods and properties that simplify accessing entities in the luis results
    public partial class ResultFromLuis : IRecognizerConvert
    {
        public Intent Intent { get; set; }
        public double Score { get; set; }
        public string Text { get; set; }
        public RecognizerResult ResultObj { get; set; }

        public ResultFromLuis()
        {
        }
        public ResultFromLuis(dynamic result)
        {
            ConvertFromDynamic(result);
        }

        public double GetScore()
        {
            return ResultObj?.Intents?.FirstOrDefault().Value?.Score ?? 0;
        }

        public string GetIntent()
        {
            return ResultObj?.Intents?.FirstOrDefault().Key ?? "";
        }

        public void Convert(dynamic result)
        {
            ResultObj = (RecognizerResult)result;
            CompleteConvert();
        }

        private void ConvertFromDynamic(dynamic result)
        {
            var x = (JToken)result.recognized;
            ResultObj = x.ToObject<RecognizerResult>();
            CompleteConvert();
        }

        private void CompleteConvert()
        {
            Text = ResultObj.Text;
            Score = GetScore();
            if (Score >= 0.50)
                switch (GetIntent())
                {
                    case "TravelExample":
                        Intent = Intent.IdeeViaggio;
                        break;
                    case "TravelBooking":
                        Intent = Intent.PrenotazioneVolo;
                        break;
                    case "BookingRestaurant":
                        Intent = Intent.PrenotazioneRistorante;
                        break;
                    case "Weather":
                        Intent = Intent.Meteo;
                        break;
                    case "None":
                    default:
                        Intent = Intent.None;
                        break;
                }
            else
            {
                Intent = Intent.None;
            }
        }
    }

    public enum Intent
    {
        IdeeViaggio = 0,
        Meteo = 1,
        PrenotazioneRistorante = 2,
        PrenotazioneVolo = 3,
        None = 4
    }



}
