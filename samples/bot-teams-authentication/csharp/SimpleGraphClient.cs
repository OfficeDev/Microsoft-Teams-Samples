// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Graph.Me.SendMail;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This class is a wrapper for the Microsoft Graph API.
    /// See: https://developer.microsoft.com/en-us/graph
    /// </summary>
    public class SimpleGraphClient
    {
        private readonly string _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleGraphClient"/> class.
        /// </summary>
        /// <param name="token">The OAuth token.</param>
        public SimpleGraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        /// <summary>
        /// Sends an email on the user's behalf using the Microsoft Graph API.
        /// </summary>
        /// <param name="toAddress">The recipient's email address.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="content">The email content.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
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
            await graphClient.Me.SendMail.PostAsync(new SendMailPostRequestBody
            {
                Message = email,
                SaveToSentItems = true
            });
        }

        /// <summary>
        /// Gets recent mail for the user using the Microsoft Graph API.
        /// </summary>
        /// <returns>An array of recent messages.</returns>
        public async Task<Message[]> GetRecentMailAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var messages = await graphClient.Me.MailFolders["Inbox"].Messages.GetAsync();
            return messages.Value.Take(5).ToArray();
        }

        /// <summary>
        /// Gets information about the user.
        /// </summary>
        /// <returns>The user information.</returns>
        public async Task<User> GetMeAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.GetAsync();
            return me;
        }

        /// <summary>
        /// Gets information about the user's manager.
        /// </summary>
        /// <returns>The manager information.</returns>
        public async Task<User> GetManagerAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var manager = await graphClient.Me.Manager.GetAsync() as User;
            return manager;
        }

        // // Gets the user's photo
        // public async Task<PhotoResponse> GetPhotoAsync()
        // {
        //     HttpClient client = new HttpClient();
        //     client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
        //     client.DefaultRequestHeaders.Add("Accept", "application/json");

        //     using (var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value"))
        //     {
        //         if (!response.IsSuccessStatusCode)
        //         {
        //             throw new HttpRequestException($"Graph returned an invalid success code: {response.StatusCode}");
        //         }

        //         var stream = await response.Content.ReadAsStreamAsync();
        //         var bytes = new byte[stream.Length];
        //         stream.Read(bytes, 0, (int)stream.Length);

        //         var photoResponse = new PhotoResponse
        //         {
        //             Bytes = bytes,
        //             ContentType = response.Content.Headers.ContentType?.ToString(),
        //         };

        //         if (photoResponse != null)
        //         {
        //             photoResponse.Base64String = $"data:{photoResponse.ContentType};base64," +
        //                                          Convert.ToBase64String(photoResponse.Bytes);
        //         }

        //         return photoResponse;
        //     }
        // }

        /// <summary>
        /// Gets an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <returns>The authenticated GraphServiceClient.</returns>
        public class SimpleAccessTokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;

            public SimpleAccessTokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> context = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_accessToken);
            }

            public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
        }

        private GraphServiceClient GetAuthenticatedClient()
        {
            var tokenProvider = new SimpleAccessTokenProvider(_token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }
    }
}
