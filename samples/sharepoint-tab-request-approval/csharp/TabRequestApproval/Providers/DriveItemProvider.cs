// <copyright file="DriveItemProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;
    using NuGet.Protocol;
    using TabActivityFeed.Helpers;
    using TabRequestApproval.Helpers;
    using TabRequestApproval.Model;

    /// <summary>
    /// Represents the drive item provider.
    /// </summary>
    public class DriveItemProvider : IDriveItemProvider
    {
        /// <summary>
        /// Represents the small file size boundary.
        /// </summary>
        private const long SmallFileSizeBoundary = 4000000;

        /// <summary>
        /// Represents the appsettings.json file details.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Represents the auth provider.
        /// </summary>
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Represents the HTTP client factory.
        /// </summary>
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriveItemProvider"/> class.
        /// </summary>
        /// <param name="authProvider">Represents the auth provider.</param>
        /// <param name="configuration">Represents the appsettings.json file settings.</param>
        /// <param name="httpClientFactory">Represents the HTTP client factory.</param>
        public DriveItemProvider(IAuthProvider authProvider, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Creates a file.
        /// A file is considered a drive item.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="parentId">Represents the parent id.</param>
        /// <param name="name">Represents the name.</param>
        /// <param name="stream">Represents the stream.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task CreateFileAsync(string accessToken, string driveId, string parentId, string name, Stream stream)
        {
            try
            {
                // Upload small file> https://docs.microsoft.com/en-us/graph/api/driveitem-put-content

                // Upload large file> https://docs.microsoft.com/en-us/graph/sdks/large-file-upload
                GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

                if (stream.Length < SmallFileSizeBoundary)
                {
                    await UploadSmallFileAsync(graphServiceClient, driveId, parentId, name, stream).ConfigureAwait(false);
                }
                else
                {
                    await UploadLargeFileAsync(graphServiceClient, driveId, parentId, name, stream).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create file. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all items within the specified drive with the specified item id.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id. Default value is null. </param>
        /// <returns>A collection of drive items.</returns>
        public async Task<ICollection<DriveItem>> GetDriveItemsAsync(string accessToken, string driveId, string itemId = null)
        {
            try
            {
                GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

                IDriveItemChildrenCollectionPage driveItems;

                // If the itemId is null, this means that driveItems are existing in the root. Otherwise, they are in a drive.
                if (itemId == null)
                {
                    driveItems = await graphServiceClient.Drives[driveId].Root.Children
                        .Request()
                        .GetAsync().ConfigureAwait(false);
                }
                else
                {
                    driveItems = await graphServiceClient.Drives[driveId].Items[itemId].Children
                        .Request()
                        .GetAsync().ConfigureAwait(false);
                }

                return driveItems;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to read drive items. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the specified drive item from the specified drive.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<DriveItem> GetDriveItemAsync(string accessToken, string driveId, string itemId)
        {
            try
            {
                GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

                DriveItem driveItem = await graphServiceClient.Drives[driveId].Items[itemId]
                    .Request()
                    .GetAsync().ConfigureAwait(false);

                return driveItem;
            }
            catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (ServiceException ex)
            {
                throw new Exception($"Unable to read drive item. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the content of the specified drive item.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <param name="driveItem">Represents the updated drive item.</param>
        /// <returns>A drive item.</returns>
        public async Task<DriveItem> UpdateDriveItemAsync(string accessToken, string driveId, string itemId, Stream driveItem)
        {
            try
            {
                GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

                DriveItem updatedDriveItem = await graphServiceClient.Drives[driveId].Root
                    .ItemWithPath($"{itemId}.txt")
                    .Content
                    .Request().PutAsync<DriveItem>(driveItem).ConfigureAwait(false);

                return updatedDriveItem;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to update drive item. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the specified drive item.
        /// </summary>
        /// <param name="accessToken">Represents the access token.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="itemId">Represents the item id.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task DeleteDriveItemAsync(string accessToken, string driveId, string itemId)
        {
            GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(accessToken);

            await graphServiceClient.Drives[driveId].Items[itemId]
                .Request()
                .DeleteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves drive item download url.
        /// </summary>
        /// <param name="accessToken">Represents access token.</param>
        /// <param name="driveId">Represents drive id.</param>
        /// <param name="itemId">Represents item id.</param>
        /// <returns>A uri representing the download url.</returns>
        /// <exception cref="Exception">Is thrown when the file url could not be obtained for any reason.</exception>
        public async Task<HttpResponseMessage> GetFileDownloadUrlAsync(string accessToken, string driveId, string itemId)
        {
            try
            {
                HttpClient client = ServiceClients.GetHttpClient(accessToken, "application/json", this.httpClientFactory);

                Uri url = new Uri($"{this.configuration["AzureAd:GraphURI"]}/v1.0/drives/{driveId}/items/{itemId}/content");

                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"File could not be downloaded. Status code {(int)response.StatusCode}. Reason: {response.ReasonPhrase}''");
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to download file. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a small file to the specified drive.
        /// </summary>
        /// <param name="graphServiceClient">Represents the graph service client.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="parentId">Represents the parent id.</param>
        /// <param name="name">Represents the name.</param>
        /// <param name="fileStream">Represents the stream.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private static async Task UploadSmallFileAsync(GraphServiceClient graphServiceClient, string driveId, string parentId, string name, Stream fileStream)
        {
            try
            {
                DriveItem driveItem = await graphServiceClient.Drives[driveId].Root
                    .ItemWithPath($"{name}.txt")
                    .Content
                    .Request().PutAsync<DriveItem>(fileStream).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to upload small file. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a large file to the specified drive.
        /// </summary>
        /// <param name="graphServiceClient">Represents the graph service client.</param>
        /// <param name="driveId">Represents the drive id.</param>
        /// <param name="parentId">Represents the parent id.</param>
        /// <param name="name">Represents the name.</param>
        /// <param name="fileStream">Represents the stream.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private static async Task UploadLargeFileAsync(GraphServiceClient graphServiceClient, string driveId, string parentId, string name, Stream fileStream)
        {
            // Use properties to specify the conflict behavior.
            // In this case, replace.
            var uploadProps = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", "replace" },
                },
            };

            UploadSession uploadSession = await graphServiceClient.Drives[driveId].Items[parentId]
                .ItemWithPath(name)
                .CreateUploadSession(uploadProps)
                .Request()
                .PostAsync().ConfigureAwait(false);

            /* Max slice size must be a multiple of 320 KiB.
            * The recommended fragment size is between 5-10 MiB.
            * https://docs.microsoft.com/en-us/graph/api/driveitem-createuploadsession?view=graph-rest-1.0
            * 320 is the KiB multiple. 1024 is the number of KiB to make a MiB. 16 is the factor.
            */
            int maxSliceSize = 320 * 1024 * 16;
            var fileUploadTask =
                new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, maxSliceSize);

            long totalLength = fileStream.Length;

            // Create a callback that is invoked after each slice is uploaded.
            IProgress<long> progress = new Progress<long>(prog =>
            {
                Console.WriteLine($"Uploaded {prog} bytes of {totalLength} bytes");
            });

            try
            {
                UploadResult<DriveItem> uploadResult = await fileUploadTask.UploadAsync(progress).ConfigureAwait(false);

                Console.WriteLine(uploadResult.UploadSucceeded ?
                    $"Upload complete, item ID: {uploadResult.ItemResponse.Id}" :
                    "Upload failed");
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error uploading: {ex.ToString()}");
            }
        }
    }
}
