using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using WeatherBot.Helper;

namespace WeatherBot.Dialogs
{
    [Serializable]
    public class CurrentLocationDialog : IDialog<object>
    {
        private string _latitude;
        private string _longitude;

        public CurrentLocationDialog(string latitude, string longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var weatherHelper = new LocationWeatherHelper(_latitude, _longitude);
            var cardObj = weatherHelper.FulfillAsync();
            var card = (object)cardObj;
            await MessageReceivedAsync(context, card);
        }

        private async Task MessageReceivedAsync(IDialogContext context, object actionResult)
        {
            try
            {
                IMessageActivity message = null;
                var obj = (Task<object>)actionResult;
                var weatherCard = (AdaptiveCards.AdaptiveCard)obj.Result;
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            context.Wait(this.MessageReceivedAsync);
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