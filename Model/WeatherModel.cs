using System;
using System.Collections.Generic;
using System.Linq;

namespace QnABot.Model
{
    public class WeatherModel
    {
        public string Cod { get; set; }
        public int Message { get; set; }
        public int Cnt { get; set; }
        public List<WeatherListModel> List { get; set; }
        public City City { get; set; }

        public WeatherModel() { }

        public WeatherModel(bool error)
        {
            Cod = error ? "404" : "200";
        }

        public WeatherListModel GetWeatherForDate(DateTime date)
        {
            return List?.FirstOrDefault(x => DateTime.Parse(x.Dt_txt) >= date);
        }
    }

    public class Main
    {
        public double Temp { get; set; }
        public double Feels_like { get; set; }
        public double Temp_min { get; set; }
        public double Temp_ax { get; set; }
        public int Pressure { get; set; }
        public int Sea_level { get; set; }
        public int Grnd_level { get; set; }
        public int Humidity { get; set; }
        public double Temp_kf { get; set; }

        public override string ToString()
        {
            return $"Temperatura: {(int)(Temp - 273.15)} °C. Pressione atmosferica: {Pressure}.";
        }
    }

    public class Weather
    {
        public int Id { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }

        public override string ToString()
        {
            return $"Descrizione: {Description}.";
        }
    }

    public class Clouds
    {
        public int All { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }
        public int Deg { get; set; }

        public override string ToString()
        {
            return $"Sezione vento: Velocità: {Speed} Nodi. Gradi: {Deg}.";
        }
    }

    public class Sys
    {
        public string Pod { get; set; }
    }

    public class WeatherListModel
    {
        public int Dt { get; set; }
        public Main Main { get; set; }
        public List<Weather> Weather { get; set; }
        public Clouds Clouds { get; set; }
        public Wind Wind { get; set; }
        public int Visibility { get; set; }
        public double Pop { get; set; }
        public Sys Sys { get; set; }
        public string Dt_txt { get; set; }

        public override string ToString()
        {
            string obj = $"Data: {Dt_txt}";
            obj += Main.ToString();
            obj += Wind.ToString();
            obj += Weather.ToString();
            return obj;
        }
    }

    public class Coord
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Coord Coord { get; set; }
        public string Country { get; set; }
        public int Population { get; set; }
        public int Timezone { get; set; }
        public int Sunrise { get; set; }
        public int Sunset { get; set; }
    }


}
