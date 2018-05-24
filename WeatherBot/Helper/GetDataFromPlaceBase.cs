using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.CognitiveServices.LuisActionBinding;

namespace WeatherBot.Helper
{
    [Serializable]
    public abstract class GetDataFromPlaceBase : BaseLuisAction
    {
        /// <summary>
        /// Location (Required)
        /// </summary>
        [Required(ErrorMessage = "I couldn't find a location :( Please try again like 'what is the weather in Ankara'")]
        public string Place { get; set; }
    }
}