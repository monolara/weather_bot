using System;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.CognitiveServices.LuisActionBinding.Bot;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using WeatherBot.Helper;

namespace WeatherBot.Dialogs
{
    [Serializable]
    public class LuisLocationDialog : LuisActionDialog<object>
    {

        public LuisLocationDialog() : base(
            new Assembly[] { typeof(LocationWeatherHelper).Assembly },
            (action, context) =>
            {

            },
            new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LUIS_ModelId"], ConfigurationManager.AppSettings["LUIS_SubscriptionKey"])))
        {
        }
        [LuisIntent("WeatherInPlace")]
        public async Task WeatherInPlaceActionHandlerAsync(IDialogContext context, object actionResult)
        {
            IMessageActivity message = null;
            var weatherCard = (AdaptiveCards.AdaptiveCard)actionResult;
            if (weatherCard == null)
            {
                message = context.MakeMessage();
                message.Text = $"I couldn't find the weather for '{context.Activity.AsMessageActivity().Text}'.  Are you sure that's a real city?";
            }
            else
            {
                message = GetMessage(context, weatherCard, "Weather card");
            }

            await context.PostAsync(message);
        }

        private IMessageActivity GetMessage(IDialogContext context, AdaptiveCards.AdaptiveCard card, string cardName)
        {
            var message = context.MakeMessage();
            if (message.Attachments == null)
                message.Attachments = new List<Attachment>();

            var attachment = new Attachment()
            {
                Content = card,
                ContentType = AdaptiveCards.AdaptiveCard.ContentType,// "application/vnd.microsoft.card.adaptive",
                Name = cardName
            };
            message.Attachments.Add(attachment);
            return message;
        }

    }
}