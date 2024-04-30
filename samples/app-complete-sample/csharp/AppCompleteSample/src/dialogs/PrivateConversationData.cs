using Microsoft.Bot.Schema;

namespace AppCompleteSample.src.dialogs
{
    public class PrivateConversationData
    {
        public string AuthTokenKey { get; set; }
        public ConversationReference PersistedCookie { get; set; }
        public ConversationReference PersistedCookieVSTS { get; set; }
        public string Name { get; set; }
        public string VSTSAuthTokenKey { get; set; }
    }

    public class UserConversationState
    { 
        public bool IsWelcomeCardSent { get; set; }
    }
}