using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ReporterPlus.Helpers
{
    public static class OutlookConnector
    {
        public static async Task SendMailAsync(string senderAddress, string recipientAddress, string cardJsonString, string mailSubject)
        {
            var authClient1 = PublicClientApplicationBuilder.Create(Constants.MicrosoftAppId).WithTenantId(Constants.TenantId).WithRedirectUri(Constants.BaseUrl).Build();
            var app = ConfidentialClientApplicationBuilder.Create(Constants.MicrosoftAppId)
                      .WithClientSecret(Constants.MicrosoftAppPassword)
                      .WithAuthority(new Uri($"{"https://login.microsoftonline.com"}/{Constants.TenantId}"))
                      .Build();

            var result = await app.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                        return Task.FromResult(0);
                    }));

            // Create a recipient
            var toRecipient = new Recipient()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = recipientAddress
                }
            };

            // Create the message
            var actionableMessage = new Message()
            {
                Subject = mailSubject,
                ToRecipients = new List<Recipient>() { toRecipient },
                Body = new ItemBody()
                {
                    ContentType = BodyType.Html,
                    Content = LoadActionableMessageBody(cardJsonString)
                },
                Attachments = new MessageAttachmentsCollectionPage()
            };

            // Send the mail
            await graphClient.Users[senderAddress]
                            .SendMail(actionableMessage, null)
                            .Request()
                            .PostAsync();
        }

        private static string LoadActionableMessageBody(string cardJsonString)
        {
            var cardJson = JObject.Parse(cardJsonString);
            string originatorId = Constants.OriginatorId;
            if (!string.IsNullOrEmpty(originatorId))
            {
                var originator = cardJson.SelectToken("originator");
                if (originator != null)
                {
                    cardJson["originator"] = originatorId;
                }
                else
                {
                    cardJson.Add(new JProperty("originator", originatorId));
                }
            }
            string files = System.IO.File.ReadAllText("./MailBody/MessageBody.html").ToString();
            return string.Format(files, cardJson.ToString());
        }
    }
}
