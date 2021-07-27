// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Microsoft.BotBuilderSamples
{
    // This class is a wrapper for the Microsoft Graph API
    // See: https://developer.microsoft.com/en-us/graph
    public class SimpleGraphClient
    {
        private readonly string _token;

        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        // Sends an email on the users behalf using the Microsoft Graph API
        public async Task SendMailAsync(string toAddress, string subject, string content)
        {
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                throw new ArgumentNullException(nameof(toAddress));
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            var graphClient = GetAuthenticatedClient();
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

            // Create the message.
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

            // Send the message.
            await graphClient.Me.SendMail(email, true).Request().PostAsync();
        }

        // Gets mail for the user using the Microsoft Graph API
        public async Task<Message[]> GetRecentMailAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var messages = await graphClient.Me.MailFolders.Inbox.Messages.Request().GetAsync();
            return messages.Take(5).ToArray();
        }

        public async Task<string> GetPhotoAsync()
        {
            var graphClient = GetAuthenticatedClient();
            try
            {
                var stream = await graphClient.Me.Photo.Content
                            .Request()
                            .GetAsync();

                MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                byte[] buffer = ms.ToArray();
                string result = Convert.ToBase64String(buffer);
                string imgDataURL = string.Format("data:image/png;base64,{0}", result);
                return imgDataURL;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return "http://adaptivecards.io/content/cats/1.png";
            }
        }

        public async Task<string> GetPublicURLForProfilePhoto(string id)
        {
            var graphClient = GetAuthenticatedClient();
            try
            {
                var stream = await graphClient.Me.Photo.Content
                            .Request()
                            .GetAsync();

                var fileName = id + "-ProflePhoto.png";
                string imagePath = Path.Combine(".", "wwwroot", "photos");

                if (!System.IO.Directory.Exists(imagePath))
                    System.IO.Directory.CreateDirectory(imagePath);

                imagePath = Path.Combine(imagePath, fileName);

                using (var fileStream = System.IO.File.Create(imagePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
                return  "/photos/" + fileName;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return "http://adaptivecards.io/content/cats/1.png";
            }
        }

        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));
            return graphClient;
        }

        public async Task<User> GetMyProfile()
        {
            var graphClient = GetAuthenticatedClient();
            return await graphClient.Me.Request().GetAsync();
        }
    }
}
