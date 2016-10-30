using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;

namespace EmotionBotTS16
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
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var responseMsg = "こんにちは！表情分析BOTです。写真を送ってね！";

                if (activity.Attachments?.Any() == true)
                {
                    var photoUrl = activity.Attachments[0].ContentUrl;
                    var client = new HttpClient();
                    var photoStream = await client.GetStreamAsync(photoUrl);                    

                    const string emotionApiKey = "YOUR_SUBSCRIPTION_KEY";
                    EmotionServiceClient emotionServiceClient = new EmotionServiceClient(emotionApiKey);

                    try
                    {
                        Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(photoStream);
                        Scores emotionScores = emotionResult[0].Scores;
                        responseMsg = Math.Ceiling(emotionScores.Happiness * 100) + "% の笑顔ですね！";
                    }
                    catch (Exception e)
                    {
                        responseMsg = "表情を分析できませんでした";
                    }
                }

                Activity reply = activity.CreateReply(responseMsg);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
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

            return null;
        }
    }
}