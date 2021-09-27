using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace FetchGroupChatMessagesWithRSC.helper
{
    public class GetChatHelper
    {
        public static async Task<IChatMessagesCollectionPage> GetGroupChatMessage(ITurnContext turnContext, TokenResponse tokenResponse, string Chatid)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }
            try
            {
                var client = new SimpleGraphClient(tokenResponse.Token);
                var messages = await client.GetUserChatMessages(Chatid);
                return messages;
            }
            catch (ServiceException ex)
            {
                // This is where app is already installed but we don't have conversation reference.
                throw ex;
            }
        }
    }
}
