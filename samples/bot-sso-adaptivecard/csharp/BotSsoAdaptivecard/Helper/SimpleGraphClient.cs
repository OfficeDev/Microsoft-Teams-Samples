// <copyright file="SimpleGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Graph.Me.SendMail;

namespace BotSsoAdaptivecard.Helper
{
    // This class is a wrapper for the Microsoft Graph API
    // See: https://developer.microsoft.com/en-us/graph
    public class GraphServiceClientHelper
    {
        private readonly string _token;

        public GraphServiceClientHelper(string token)
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
        /// <param name="toAddress">Recipient email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="content">Body content of the email.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"></exception>
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

            try
            {
                // Send the message.
                await graphClient.Me.SendMail.PostAsync(new SendMailPostRequestBody
                {
                    Message = email,
                    SaveToSentItems = true
                });
            }
            catch (ServiceException ex)
            {
                // Handle exception
                Console.WriteLine($"Error sending mail: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the most recent mail for the user.
        /// </summary>
        /// <returns>A list of the 5 most recent messages.</returns>
        public async Task<Message[]> GetRecentMailAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var messages = await graphClient.Me.MailFolders["Inbox"].Messages.GetAsync().ConfigureAwait(false);
            return messages.Value.Take(5).ToArray();
        }

        /// <summary>
        /// Gets information about the current authenticated user.
        /// </summary>
        /// <returns>User information.</returns>
        public async Task<User> GetMeAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.GetAsync().ConfigureAwait(false);
            return me;
        }

        /// <summary>
        /// Gets the user's photo in base64 format.
        /// </summary>
        /// <returns>The base64-encoded photo string, or an empty string if no photo is available.</returns>
        public async Task<string> GetPhotoAsync()
        {
            var graphClient = GetAuthenticatedClient();
            try
            {
                var photo = await graphClient.Me.Photo.Content.GetAsync();
                if (photo != null)
                {
                    MemoryStream ms = new MemoryStream();
                    photo.CopyTo(ms);
                    byte[] buffers = ms.ToArray();
                    string imgDataURL = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(buffers));
                    return imgDataURL;
                }
                else
                {
                    return ""; // Return empty string if no photo is available.
                }
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error fetching photo: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Returns an authenticated Microsoft Graph client using the token issued to the user.
        /// </summary>
        /// <returns>An authenticated instance of GraphServiceClient.</returns>
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
