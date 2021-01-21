using Newtonsoft.Json;
using QnABot.Interfaces;
using QnABot.Model;
using System.Net.Http;
using System.Threading.Tasks;

namespace QnABot.Classes
{
    public class Weather : IWeather
    {
        private readonly WeatherSettings weatherSettings;

        public Weather(WeatherSettings weatherSettings)
        {
            this.weatherSettings = weatherSettings;
        }

        public async Task<WeatherModel> GetWeather(string city)
        {
            HttpClient client = new HttpClient();
            try
            {
                string link = string.Format(weatherSettings.WeatherApiLink, city);
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WeatherModel>(responseBody);
            }
            catch
            {
                return new WeatherModel(true);
            }            
        }
    }
}
