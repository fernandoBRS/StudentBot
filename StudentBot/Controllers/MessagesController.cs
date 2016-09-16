using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using StudentBot.Models;

namespace StudentBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var messageType = new MessageType(activity, connector);

                await messageType.ValidateMessageAsync();

                // state sample

                //var stateClient = activity.GetStateClient();

                //var userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                //var reply = activity.CreateReply("count = " + userData.GetProperty<int>("SentGreeting").ToString());
                //await connector.Conversations.ReplyToActivityAsync(reply);

                //userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                //userData.SetProperty<int>("SentGreeting", userData.GetProperty<int>("SentGreeting") + 1);
                //await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

            }
            else
            {
                HandleSystemMessage(activity);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private static async void echo(Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            // calculate something for us to return
            var length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user

            var reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
            await connector.Conversations.ReplyToActivityAsync(reply);
        }
        
        private static void HandleSystemMessage(IActivity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
        }
    }
}