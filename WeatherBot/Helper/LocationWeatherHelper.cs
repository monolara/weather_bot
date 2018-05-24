using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using AdaptiveCards;
using APIXULib;
using Microsoft.Bot.Builder.CognitiveServices.LuisActionBinding;

namespace WeatherBot.Helper
{
    [Serializable]
    [LuisActionBinding("WeatherInPlace", FriendlyName = "Get the Weather in a location")]
    public class LocationWeatherHelper : GetDataFromPlaceBase
    {
        private static string _latitude;
        private static string _longitude;

        public LocationWeatherHelper()
        {

        }

        public LocationWeatherHelper(string lat = null, string lon = null)
        {
            if (lat != null && lon != null)
            {
                _latitude = lat;
                _longitude = lon;
            }

        }
        
        public override Task<object> FulfillAsync()
        {
            if (_latitude != null && _longitude != null)
            {
                var result = GetCard(string.Empty, _latitude, _longitude);
                return Task.FromResult((object)result);
            }
            else
            {
                var result = GetCard(this.Place);
                return Task.FromResult((object)result);
            }
        }

        public static AdaptiveCard GetCard(string place, string lat = null, string lon = null)
        {
            bool isLocation = false;
            WeatherModel model = null;
            if (!string.IsNullOrWhiteSpace(place) && lat == null && lon == null)
            {
            model = new Repository().GetWeatherData(ConfigurationManager.AppSettings["APIXUKey"], GetBy.CityName, place, Days.Five);

            }
            else if (string.IsNullOrEmpty(place) && lat != null && lon != null)
            {
                isLocation = true;
                model = new Repository().GetWeatherDataByLatLong(ConfigurationManager.AppSettings["APIXUKey"], lat, lon, Days.Five);
                _latitude = null;
                _longitude = null;
            }

            var card = new AdaptiveCard();
            if (model != null)
            {
                if (model.current != null)
                {
                    card.Speak = $"<s>Today the temperature is {model.current.temp_c} in {(!isLocation ? model.location.name : model.location.region)}</s><s>Winds are {model.current.wind_mph} miles per hour from the {model.current.wind_dir}</s>";
                }

                if (model.forecast != null && model.forecast.forecastday != null)
                {
                    AddCurrentWeather(model, card, isLocation);
                    AddForecast(!isLocation ? model.location.name : model.location.region, model, card);
                    return card;
                }
            }
            return null;
        }

        private static void AddCurrentWeather(WeatherModel model, AdaptiveCard card, bool isLocation = false)
        {
            var current = new ColumnSet();
            card.Body.Add(current);

            var currentColumn = new Column();
            current.Columns.Add(currentColumn);
            currentColumn.Size = "35";

            var currentImage = new Image();
            currentColumn.Items.Add(currentImage);
            currentImage.Url = GetIconUrl(model.current.condition.icon);

            var currentColumn2 = new Column();
            current.Columns.Add(currentColumn2);
            currentColumn2.Size = "65";

            string date = DateTime.Parse(model.current.last_updated).DayOfWeek.ToString();

            AddTextBlock(currentColumn2, $"{(!isLocation ? model.location.name : model.location.region)} ({date})", TextSize.Large, false);
            AddTextBlock(currentColumn2, $"{model.current.temp_c.ToString().Split('.')[0]}° C", TextSize.Large);
            AddTextBlock(currentColumn2, $"{model.current.condition.text}", TextSize.Medium);
            AddTextBlock(currentColumn2, $"Winds {model.current.wind_mph} mph {model.current.wind_dir}", TextSize.Medium);
        }
        private static void AddForecast(string place, WeatherModel model, AdaptiveCard card)
        {
            var forecast = new ColumnSet();
            card.Body.Add(forecast);

            foreach (var day in model.forecast.forecastday)
            {
                if (DateTime.Parse(day.date).DayOfWeek != DateTime.Parse(model.current.last_updated).DayOfWeek)
                {
                    var column = new Column();
                    AddForcastColumn(forecast, column, place);
                    AddTextBlock(column, DateTimeOffset.Parse(day.date).DayOfWeek.ToString().Substring(0, 3), TextSize.Medium);
                    AddImageColumn(day, column);
                    AddTextBlock(column, $"{day.day.mintemp_c.ToString().Split('.')[0]}/{day.day.maxtemp_c.ToString().Split('.')[0]}", TextSize.Medium);
                }
            }
        }
        private static void AddImageColumn(Forecastday day, Column column)
        {
            var image = new Image();
            image.Size = ImageSize.Auto;
            image.Url = GetIconUrl(day.day.condition.icon);
            column.Items.Add(image);
        }
        private static string GetIconUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (url.StartsWith("http"))
                return url;
            //some clients do not accept \\
            return "https:" + url;
        }
        private static void AddForcastColumn(ColumnSet forecast, Column column, string place)
        {
            forecast.Columns.Add(column);
            column.Size = "20";
            var action = new OpenUrlAction();
            action.Url = $"https://www.bing.com/search?q=forecast in {place}";
            column.SelectAction = action;
        }

        private static void AddTextBlock(Column column, string text, TextSize size, bool isSubTitle = true)
        {
            column.Items.Add(new TextBlock()
            {
                Text = text,
                Size = size,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsSubtle = isSubTitle,
                Separation = SeparationStyle.None
            });
        }



    }
}