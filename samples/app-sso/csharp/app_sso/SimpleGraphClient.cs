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

        private async Task<string> GetPhoto()
        {
            HttpClient client = new HttpClient();
            var resp = await client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");
            var buffer = await resp.Content.ReadAsByteArrayAsync();
            var byteArray = buffer.ToArray();
            string base64String = Convert.ToBase64String(byteArray);
            return base64String;

        }

        public  async Task<byte[]> GetStreamWithAuthAsync()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            using (var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);

                    return bytes;
                }
                else
                    return null;
            }
        }

        public async Task<string> GetPhotoAsync()
        {
            //Image Ima
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
                throw;
            }
        }


        public  async Task<Stream> GetCurrentUserPhotoStreamAsync()
        {
            Stream currentUserPhotoStream = null;
            var graphClient = GetAuthenticatedClient();
            try
            {
                currentUserPhotoStream = await graphClient.Me.Photo.Content.Request().GetAsync();

            }
            // If the user account is MSA (not work or school), the service will throw an exception.
            catch (Exception)
            {
                return null;
            }

            return currentUserPhotoStream;

        }

        public async Task<DriveItem> UploadFileToOneDriveAsync(byte[] file)
        {
            DriveItem uploadedFile = null;
            var graphClient = GetAuthenticatedClient();

            try
            {
                MemoryStream fileStream = new MemoryStream(file);
                uploadedFile = await graphClient.Me.Drive.Root.ItemWithPath("me.png").Content.Request().PutAsync<DriveItem>(fileStream);
            }
            catch (ServiceException)
            {
                return null;
            }

            return uploadedFile;
        }

        public  async Task<Permission> GetSharingLinkAsync(string Id)
        {
            Permission permission = null;
            var graphClient = GetAuthenticatedClient();
            try
            {
                permission = await graphClient.Me.Drive.Items[Id].CreateLink("view").Request().PostAsync();
            }
            catch (ServiceException)
            {
                return null;
            }

            return permission;
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
