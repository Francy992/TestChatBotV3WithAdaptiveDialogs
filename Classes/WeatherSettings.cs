using Microsoft.Extensions.Configuration;

namespace QnABot.Classes
{
    public class WeatherSettings
    {
        public string WeatherApiKey { get; set; }
        public string WeatherApiLink { get; set; }

        public WeatherSettings(IConfiguration configuration)
        {
            WeatherApiKey = configuration["WeatherApiKey"];
            WeatherApiLink = string.Format(configuration["WeatherApiLink"], "{0}", WeatherApiKey);
        }
    }
}
