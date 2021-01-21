using Microsoft.Bot.Builder;
using System.Linq;

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

        public void Convert(dynamic result)
        {
            RecognizerResult resultObj = (RecognizerResult)result;
            ResultObj = resultObj;
            Text = resultObj.Text;
            Score = resultObj?.Intents?.FirstOrDefault().Value?.Score ?? 0;
            if(Score >= 0.50)
                switch (resultObj?.Intents?.FirstOrDefault().Key)
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
