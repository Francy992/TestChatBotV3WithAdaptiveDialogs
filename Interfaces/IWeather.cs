using QnABot.Model;
using System.Threading.Tasks;

namespace QnABot.Interfaces
{
    public interface IWeather
    {
        Task<WeatherModel> GetWeather(string city);
    }
}
