// <copyright file="ContainerProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TabActivityFeed.Helpers;
    using TabActivityFeed.Models;
    using TabRequestApproval.Helpers;
    using Container = TabActivityFeed.Models.Container;

    /// <summary>Provides helper methods built over MS Graph SDK.</summary>
    public class ContainerProvider : IContainerProvider
    {
        /// <summary>
        /// Represents appsettings.json file.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Represents the HTTP client factory.
        /// </summary>
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Represents the auth provider.
        /// </summary>
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Represents the logger to use in this provider.
        /// </summary>
        private readonly ILogger<ContainerProvider> logger;

        /// <summary>
        /// Represents the graph container endpoint.
        /// </summary>
        private readonly string graphContainersEndpoint = "beta/storage/fileStorage/containers";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerProvider"/> class.
        /// Creates a container provider object. This exposes container operations.
        /// </summary>
        /// <param name="configuration">Represents the appsettings.json file details.</param>
        /// <param name="httpClientFactory">Represents the HTTP Client Factory.</param>
        /// <param name="logger">Represents the logger to be used in this provider.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        public ContainerProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ContainerProvider> logger, IAuthProvider authProvider)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
            this.AppInstallationScopeDetailsDictionary = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets represents a dictionary that stores a scope-id key-value pair.
        /// </summary>
        private ConcurrentDictionary<string, string> AppInstallationScopeDetailsDictionary { get; set; }

        /// <summary>
        /// Retrieves all existing containers.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <returns>An IEnumerable of all existing containers.</returns>
        /// <exception cref="Exception">Is thrown when the containers could not be retrieved for any reason.</exception>
        public async Task<IEnumerable<Container>> GetAllContainersAsync(string accessToken)
        {
            string containerTypeId = this.configuration.GetValue<string>("AzureAd:ContainerTypeId");

            HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

            Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}?$filter=containerTypeId eq {containerTypeId}");

            HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"List of containers could not be retrieved. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
            }

            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject deserialized = JsonConvert.DeserializeObject<JObject>(content);
            JArray array = deserialized.Value<JArray>("value");
            return array.ToObject<List<Container>>();
        }

        /// <summary>
        /// Retrieves a container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <returns>The specified container.</returns>
        /// <exception cref="Exception">Is thrown when the specified container could not be retrieved for any reason.</exception>
        public async Task<Container> GetContainerAsync(string accessToken, string containerId)
        {
            HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

            Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}");

            HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Container could not be retrieved. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
            }

            return await response.Content.ReadAsAsync<Container>().ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the specified container. Creates one if it doesn't exist.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Container> CreateOrGetContainerAsync(string accessToken, string containerId, string teamsAppInstallationScopeId)
        {
            try
            {
                if (containerId == null)
                {
                    // Create container if none exists.
                    Container containerToCreate = new Container()
                    {
                        displayName = "Display Name",
                        description = "Container to store all request related data.",
                        containerTypeId = this.configuration["AzureAd:ContainerTypeId"],
                        createdDateTime = DateTime.UtcNow.ToString(),
                        status = "inactive",
                    };

                    HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

                    HttpResponseMessage response = await client.PostAsJsonAsync($"{this.graphContainersEndpoint}", containerToCreate).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Container could not be created. Status Code: {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
                    }

                    Container createdContainer = await response.Content.ReadAsAsync<Container>().ConfigureAwait(false);

                    this.AppInstallationScopeDetailsDictionary.TryAdd(teamsAppInstallationScopeId, createdContainer.id);

                    return createdContainer;
                }
                else
                {
                    Container container = await this.GetContainerAsync(accessToken, containerId).ConfigureAwait(false);

                    return container;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to read or create container. Reason: {ex.Message}.");
            }
        }

        /// <summary>
        /// Deletes the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">Is thrown when the specified container could not be deleted for any reason.</exception>
        public async Task DeleteContainerAsync(string accessToken, string containerId, string teamsAppInstallationScopeId)
        {
            try
            {
                HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

                Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}");

                HttpResponseMessage response = await client.DeleteAsync(uri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Container could not be deleted. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
                }

                this.AppInstallationScopeDetailsDictionary.Remove(teamsAppInstallationScopeId, out string removedContainerId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Container could not be deleted. Reason: ${ex.Message}");
            }
        }

        /// <summary>
        /// Activates the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">Is thrown when the specified container could not be deleted for any reason.</exception>
        public async Task ActivateContainerAsync(string accessToken, string containerId)
        {
            try
            {
                HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

                Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}/activate");

                HttpResponseMessage response = await client.PostAsync(uri, null).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Container could not be activated. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Container could not be activated. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the container id associated with the teams app installation scope.
        /// </summary>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope id.</param>
        /// <returns>A container id associated with the teams app installation scope.</returns>
        public string GetContainerIdFromTeamsAppInstallationScopeId(string teamsAppInstallationScopeId)
        {
            bool doesContainerExist = this.AppInstallationScopeDetailsDictionary.TryGetValue(teamsAppInstallationScopeId, out string containerId);

            if (doesContainerExist)
            {
                return containerId;
            }
            else
            {
                return null;
            }
        }
    }
}
