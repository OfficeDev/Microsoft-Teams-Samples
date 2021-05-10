// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using MeetingExtension_SP.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MeetingExtension_SP.Repositories
{
    public class AuthProvider //:IAuthProvider
    {
        private readonly string tenant;
        private readonly string clientID;
        private readonly string clientSecret;
        private readonly string sharePointEndPoint;
        private readonly string sharePointResource;
        private readonly string url; 


        public AuthProvider(IConfiguration configuration)
        {
            this.tenant = configuration["SPTenantId"];
            this.clientID = configuration["SharePointClientID"];
            this.clientSecret = configuration["SharePointClientSecret"];
            this.sharePointEndPoint = configuration["SharePointEndPoint"];
            this.sharePointResource = configuration["SharePointResource"];
            this.url = configuration["AccessTokenUrl"];
        }

        /// <summary>
        /// Get access token
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync()
        {
            var accessToken = string.Empty;

            var body = $"grant_type=client_credentials&client_id={this.clientID}@{this.tenant}&client_secret={this.clientSecret}&resource={this.sharePointResource}/{this.sharePointEndPoint}@{this.tenant}";
            try
            {
                HttpClient httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(responseBody);
                }

                accessToken = JsonConvert.DeserializeObject<TokenResponse>(responseBody).access_token;
                return accessToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
