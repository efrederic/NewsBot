using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using NewsAPI;

namespace TestBotApplication {
    [BotAuthentication]
    public class MessagesController : ApiController {

        public static readonly NewsApiClient NewsApiClient = new NewsApiClient("c3445b597bf24b71819dd8bdd2b33632");

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity) {
            if (activity.GetActivityType() == ActivityTypes.Message) {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            } else {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message) {
            string messageType = message.GetActivityType();
            if (messageType == ActivityTypes.DeleteUserData) {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            } else if (messageType == ActivityTypes.ConversationUpdate) {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            } else if (messageType == ActivityTypes.ContactRelationUpdate) {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            } else if (messageType == ActivityTypes.Typing) {
                // Handle knowing that the user is typing
            } else if (messageType == ActivityTypes.Ping) {
            }

            return null;
        }
    }
}