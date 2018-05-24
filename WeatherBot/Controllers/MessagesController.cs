using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using WeatherBot.Dialogs;

namespace WeatherBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Entities.Any() && activity.Entities.Any(a => a.Type == "Place"))
                {
                    dynamic data = JObject.Parse(activity.ChannelData.ToString());
                    string latitude;
                    string longitude;
                    if (activity.ChannelId == "facebook")
                    {
                        latitude = data.message.attachments[0].payload.coordinates["lat"].ToString();
                        longitude = data.message.attachments[0].payload.coordinates["long"].ToString();
                    }
                    else
                    {
                        latitude = data.message.location["latitude"].ToString();
                        longitude = data.message.location["longitude"].ToString(); 
                    }

                    await Conversation.SendAsync(activity, () => new CurrentLocationDialog(latitude, longitude));
                }
                else
                {
                    await Conversation.SendAsync(activity, () => new LuisLocationDialog());
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            string messageType = message.GetActivityType();
            if (messageType == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (messageType == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                IConversationUpdateActivity iConversationUpdated = message as IConversationUpdateActivity;
                {
                    ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));

                    foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                    {
                        // if the bot is added, then 
                        if (member.Id == iConversationUpdated.Recipient.Id)
                        {
                            var reply = ((Activity)iConversationUpdated).CreateReply($"Hi! I'm a Weather Bot of Dilâra Tezel.  Ask me about the weather in a city.");
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                }
            }
            else if (messageType == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (messageType == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (messageType == ActivityTypes.Ping)
            {
            }
        }
    }
}