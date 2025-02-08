// <copyright file="SimpleGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Azure.Core;
using System.Threading;
using Microsoft.Kiota.Abstractions.Authentication;

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
			await graphClient.Me.SendMail.PostAsync(new Graph.Me.SendMail.SendMailPostRequestBody() { Message = email, SaveToSentItems = true});
		}

		// Gets mail for the user using the Microsoft Graph API
		public async Task<Message[]> GetRecentMailAsync()
		{
			var graphClient = GetAuthenticatedClient();
			var messages = await graphClient.Me.Messages.GetAsync();
			return messages.Value.Take(5).ToArray();
		}

		// Get information about the user.
		public async Task<User> GetMeAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var me = await graphClient.Me.GetAsync();
            return me;
        }

        // Gets the user's photo
        public async Task<string> GetPhotoAsync()
        {
            var graphClient = GetAuthenticatedClient();
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
                return "";
            }
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {

			var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(_token));
			var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }
    }

	public class TokenProvider : IAccessTokenProvider
	{
		readonly string _token;

		public TokenProvider(string token)
		{
			_token = token;
        }
        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default,
			CancellationToken cancellationToken = default)
		{
			return Task.FromResult(_token);
		}

		public AllowedHostsValidator AllowedHostsValidator { get; }
	}
}
