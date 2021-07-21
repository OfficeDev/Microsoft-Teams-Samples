namespace ReporterPlus.Helpers
{
    public static class Constants
    {
        public static string MicrosoftAppId;
        public static string MicrosoftAppPassword;
        public static string TenantId;
        public static string ServiceUrl;
        public static string BaseUrl;
        public static string OriginatorId;
        public static string SenderEmail;
        public static string BlobContainerName;
        public static string BlobConnectionString;
        public static int MaxImagesCount = 5;
        public static string MessageExtensionCommandId = "BarCodeScanner";
        public static string ComposeExtensionFetch = "composeExtension/fetchTask";
        public static string ComposeExtensionSubmit = "composeExtension/submitAction";
        public static string TaskModuleFetch = "task/fetch";
        public static string AdaptiveCardAction = "adaptiveCard/action";
        public static string MailSubject = "New Request Assigned";
        public static string MemberGenericImageUrl = "https://devicecapabilities.blob.core.windows.net/filestorage/UserLogo.png";
        public static string PendingImageUrl = "https://adaptivecards.io/content/pending.png";
        public static string ApprovedImageUrl = "https://spsharewithme.azurewebsites.net/Images/approve1.png";
        public static string RejectedImageUrl = "https://spsharewithme.azurewebsites.net/Images/reject.png";
    }
}
