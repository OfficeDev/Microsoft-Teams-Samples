using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Graph;
using Microsoft.Graph.Me.SendMail;
using Microsoft.Graph.Models;

namespace Microsoft.BotBuilderSamples
{
    public class SimpleGraphClient
    {
        private readonly string _token;
        private readonly GraphServiceClient _graphClient;

        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            _token = token;
            _graphClient = GetAuthenticatedClient();
        }

        // Sends an email on the user's behalf using Microsoft Graph API
        public async Task SendMailAsync(string toAddress, string subject, string content)
        {
            if (string.IsNullOrWhiteSpace(toAddress)) throw new ArgumentNullException(nameof(toAddress));
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException(nameof(subject));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentNullException(nameof(content));

            var recipients = new List<Recipient>
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = toAddress,
                    },
                },
            };

            var email = new Message
            {
                Body = new ItemBody
                {
                    Content = content,
                    ContentType = BodyType.Text,
                },
                Subject = subject,
                ToRecipients = recipients,
            };

            await _graphClient.Me.SendMail.PostAsync(new SendMailPostRequestBody
            {
                Message = email,
                SaveToSentItems = true
            });
        }

        // Gets recent mail for the user
        public async Task<Message[]> GetRecentMailAsync()
        {
            var response = await _graphClient.Me.MailFolders["Inbox"].Messages.GetAsync(config =>
            {
                config.QueryParameters.Top = 5;
                config.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");
            });

            return response.Value.ToArray();
        }

        // Gets the user's profile photo as a Base64 string
        public async Task<string> GetPhotoAsync()
        {
            try
            {
                var stream = await _graphClient.Me.Photo.Content.GetAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var buffer = ms.ToArray();
                return $"data:image/png;base64,{Convert.ToBase64String(buffer)}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "http://adaptivecards.io/content/cats/1.png";
            }
        }

        // Saves the user's profile photo to a public URL
        public async Task<string> GetPublicURLForProfilePhoto(string id)
        {
            try
            {
                var stream = await _graphClient.Me.Photo.Content.GetAsync();
                var fileName = $"{id}-ProfilePhoto.png";
                var imagePath = Path.Combine(".", "wwwroot", "photos");

                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                var fullPath = Path.Combine(imagePath, fileName);

                using var fileStream = File.Create(fullPath);
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);

                return $"/photos/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "http://adaptivecards.io/content/cats/1.png";
            }
        }

        // Gets the authenticated user's profile
        public async Task<User> GetMyProfile()
        {
            return await _graphClient.Me.GetAsync();
        }

        // Creates a GraphServiceClient using a custom TokenCredential
        private GraphServiceClient GetAuthenticatedClient()
        {
            var tokenCredential = new RawTokenCredential(_token);
            return new GraphServiceClient(tokenCredential, new[] { "https://graph.microsoft.com/.default" });
        }

        // Custom TokenCredential to wrap a raw token
        private class RawTokenCredential : TokenCredential
        {
            private readonly string _accessToken;

            public RawTokenCredential(string accessToken)
            {
                _accessToken = accessToken;
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1));
            }

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new ValueTask<AccessToken>(new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1)));
            }
        }
    }
}
