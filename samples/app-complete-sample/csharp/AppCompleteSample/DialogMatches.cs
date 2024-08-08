namespace AppCompleteSample
{
    /// <summary>
    /// This contains the matching expression
    /// commands to trigger the individual dialogs in RootDialog
    /// </summary>
    public static class DialogMatches
    {
        public const string FetchRosterApiMatch = "names";

        public const string FetchRosterPayloadMatch = "roster";

        public const string PromptFlowGameMatch = "prompt";
        
        public const string RunQuizQuestionsMatch = "quiz";

        public const string DialogFlowMatch = "dialog flow";

        public const string HelloDialogMatch1 = "hi";
        public const string HelloDialogMatch2 = "hello";
        
        public const string AtMentionMatch1 = "at mention";
        public const string AtMentionMatch2 = "atmention";
        public const string AtMentionMatch3 = "at-mention";

        public const string Help = "help";

        public const string MultiDialog1Match1 = "multi dialog 1";

        public const string MultiDialog2Match = "multi dialog 2";

        public const string FecthLastExecutedDialogMatch = "last dialog";

        public const string Send1to1Conversation = "send message to 1:1";

        public const string SetUpTextMsg = "setup text message";
        public const string UpdateLastSetupTextMsg = "update text message";

        public const string SetUpCardMsg = "setup card message";

        public const string DisplayCards = "display cards";
        public const string StopShowingCards = "no";

        public const string DeepLinkTabCard = "deep link";

        public const string AuthSample = "auth";

        public const string Facebooklogin= "fblogin";
        public const string Facebooklogout = "fblogout";

        public const string VSTSlogin = "vstslogin";
        public const string VSTSlogout = "vstslogout";

        public const string VSTSApi = "vstsapi";

        public const string MessageBack = "msgback";

        public const string LocalTime = "localtime";

        public const string HeroCard = "hero card";
        public const string ThumbnailCard = "thumbnail card";

        public const string O365ConnectorCardDefault = "connector card";
        public const string O365ConnectorCards = "connector card (.*)";

        public const string O365ConnectorCardActionableCardDefault = "connector card actions";
        public const string O365ConnectorCardActionableCards = "connector card actions (.*)";

        public const string PopUpSignIn = "signin";

        public const string TeamInfo = "team info";

        public const string UpdateCard = "update card message";

        public const string AdaptiveCard = "adaptive card";

        public const string DisplayCardO365ConnectorCard2 = "connector card 2";

        public const string DisplayCardO365ConnectorCard3 = "connector card 3";

        public const string DisplayCardO365ConnectorActionableCard2 = "connector card actions 2";

        public const string MsteamsChannelId = "msteams";

        public const string MultiHubComposeExtensionCardType = "thumbnail";
    }
}