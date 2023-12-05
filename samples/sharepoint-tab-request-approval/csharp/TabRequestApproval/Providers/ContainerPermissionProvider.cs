// <copyright file="ContainerPermissionProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TabActivityFeed.Helpers;
    using TabActivityFeed.Models;
    using TabRequestApproval.Helpers;

    /// <summary>
    /// Container permissions provider.
    /// </summary>
    public class ContainerPermissionProvider : IContainerPermissionProvider
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
        private readonly ILogger<ContainerPermissionProvider> logger;

        /// <summary>
        /// Represents the graph container endpoint.
        /// </summary>
        private readonly string graphContainersEndpoint = "beta/storage/fileStorage/containers";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerPermissionProvider"/> class.
        /// Creates a container provider object. This exposes container operations.
        /// </summary>
        /// <param name="configuration">Represents the appsettings.json file details.</param>
        /// <param name="httpClientFactory">Represents the HTTP client factory.</param>
        /// <param name="logger">Represents the logger to be used in this provider.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        public ContainerPermissionProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<ContainerPermissionProvider> logger, IAuthProvider authProvider)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        }

        /// <summary>
        /// Creates the permissions on the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="permission">Represents the permissions on the container.</param>
        /// <returns>The permission model to be set on the specified container.</returns>
        /// <exception cref="Exception">Is thrown when the permissions could not be created on the specified container for any reason.</exception>
        public async Task<ContainerPermission> CreateContainerPermissionAsync(string accessToken, string containerId, ContainerPermission permission)
        {
            HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

            string serialized = JsonConvert.SerializeObject(permission, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            HttpContent content = new StringContent(serialized, Encoding.UTF8, "application/json");

            Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}/permissions");

            HttpResponseMessage response = await client.PostAsync(uri, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Permissions could not be added to the container. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
            }

            return await response.Content.ReadAsAsync<ContainerPermission>().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the permissions set on a specific container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <returns>The permissions set on the specified container.</returns>
        /// <exception cref="Exception">Is thrown when the permissions could not be retrieved for any reason.</exception>
        public async Task<IEnumerable<ContainerPermission>> ReadContainerPermissionAsync(string accessToken, string containerId)
        {
            HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

            Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}/permissions");

            HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Permissions could not be retrieved. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
            }

            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            JObject deserialized = JsonConvert.DeserializeObject<JObject>(content);

            JArray array = deserialized.Value<JArray>("value");

            return array.ToObject<List<ContainerPermission>>();
        }

        /// <summary>
        /// Updates the permissions on the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="permissionId">Represents the permission id.</param>
        /// <param name="role">Represents the role of the container.</param>
        /// <returns>The updated permissions on the specified container.</returns>
        /// <exception cref="Exception">Is thrown when the permissions could not be updated for any reason.</exception>
        public async Task<ContainerPermission> UpdateContainerPermissionAsync(string accessToken, string containerId, string permissionId, string role)
        {
            HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

            string json = $@"{{ ""roles"":[""{role}""]}}";

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}/permissions/{permissionId}");

            HttpResponseMessage response = await client.PatchAsync(uri, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Permission could not be updated. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
            }

            return await response.Content.ReadAsAsync<ContainerPermission>().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the permissions set on the specified container.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="containerId">Represents the container id.</param>
        /// <param name="permissionId">Represents the permission id.</param>
        /// <returns>A void response.</returns>
        /// <exception cref="Exception">Is thrown when the permissions could not be deleted for any reason.</exception>
        public async Task DeleteContainerPermissionAsync(string accessToken, string containerId, string permissionId)
        {
            HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

            Uri uri = new Uri($"{this.configuration["AzureAd:GraphURI"]}{this.graphContainersEndpoint}/{containerId}/permissions/{permissionId}");

            HttpResponseMessage response = await client.DeleteAsync(uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Permission could not be deleted. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
            }
        }
    }
}
