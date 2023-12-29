// <copyright file="ChangeNotificationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Logging;
	using Microsoft.Graph;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using NuGet.Protocol;
	using TabActivityFeed.Helpers;
	using TabActivityFeed.Models;
	using User = TabActivityFeed.Models.User;

	/// <summary>
	/// Responsible for managing and processing change notifications.
	/// </summary>
	public class ChangeNotificationProvider : IChangeNotificationProvider
    {
        /// <summary>
        /// Represents the certificate path.
        /// </summary>
        private const string CertificatePath = @".cer";

        /// <summary>
        /// Represents the certificate's private key path.
        /// </summary>
        private const string CertificatePrivateKeyPath = @".key";

        /// <summary>
        /// Represents the uninstallation event.
        /// </summary>
        private const string UninstallationEvent = "#microsoft.graph.teamsAppRemovedEventMessageDetail";

        /// <summary>
        /// Represents the installation event.
        /// </summary>
        private const string InstallationEvent = "#microsoft.graph.teamsAppInstalledEventMessageDetail";

        /// <summary>
        /// Represents the addition of a member to a channel or chat.
        /// </summary>
        private const string MemberAddedEvent = "#microsoft.graph.membersAddedEventMessageDetail";

        /// <summary>
        /// Represents the removal of a member from a channel or chat.
        /// </summary>
        private const string MemberDeletedEvent = "#microsoft.graph.membersDeletedEventMessageDetail";

        /// <summary>
        /// Represents the logger to use in this provider.
        /// </summary>
        private readonly ILogger<SubscriptionProvider> logger;

        /// <summary>
        /// Represents the auth provider.
        /// </summary>
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Represents the container provider.
        /// </summary>
        private readonly IContainerProvider containerProvider;

        /// <summary>
        /// Represents container permission provider.
        /// </summary>
        private readonly IContainerPermissionProvider containerPermissionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeNotificationProvider"/> class.
        /// Creates a subscription provider object.
        /// </summary>
        /// <param name="logger">Represents the logger.</param>
        /// <param name="containerProvider">Represents the container provider.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        /// <param name="containerPermissionProvider">Represents the container permission provider.</param>
        public ChangeNotificationProvider(IAuthProvider authProvider, ILogger<SubscriptionProvider> logger, IContainerProvider containerProvider, IContainerPermissionProvider containerPermissionProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
            this.containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));
            this.containerPermissionProvider = containerPermissionProvider ?? throw new ArgumentNullException(nameof(containerPermissionProvider));
        }

        /// <summary>
        /// Processes notification payloads for the chat subscription.
        /// </summary>
        /// <param name="pagedNotificationPayload">Represents the notification payload from the Graph API.</param>
        /// <exception cref="NotImplementedException">Is thrown because the method is not yet implemented.</exception>
        /// <returns>>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessChatsChangeNotificationAsync(PagedNotificationPayload pagedNotificationPayload)
        {
            try
            {
                // Decrypt potentially encrypted data.
                JObject decryptedData = this.DecryptNotificationPayload(pagedNotificationPayload);

                /* TODO: Currently, the subscription flow is incomplete.
                 * Feature request is out to support receiving subscriptions.
                 * Change this value for testing purposes.
                 *
                 * Once complete, access the data that is needed from the decryptedData variable above.
                 * The minimum required data is outlined below prior to the container lifecycle management function.
                 */
                string eventDetail = decryptedData["eventDetail"].ToJson();
                JObject eventDetailJson = JsonConvert.DeserializeObject<JObject>(eventDetail);

                string notificationOrigin = decryptedData["from"].ToJson();
                JObject notificationOriginJson = JsonConvert.DeserializeObject<JObject>(notificationOrigin);

                string notificationBody = decryptedData["body"].ToJson();
                JObject notificationBodyJson = JsonConvert.DeserializeObject<JObject>(notificationBody);

                /* TODO: Currently, the subscription flow is incomplete.
                 * Feature request is out to support receiving subscriptions.
                 *
                 * This value acquires the subscription event type; it represents if installation or uninstallation
                 * of the application occurs. Typically, it would be acquired from a field within the decryptedData
                 * variable. However, due to the incomplete subscription feature, this field is being acquired from
                 * the message body sent in chat.
                 *
                 * Refer to the README.md for more details.
                 */
                string requestODataType = string.Empty;
                string userImpactedId = string.Empty;
                string userData = string.Empty;
                JObject userJson = new JObject();

                if (eventDetailJson != null)
                {
                    // When updates are done to team membership or an app is installed/uninstalled.
                    requestODataType = eventDetailJson["@odata.type"].ToString();

                    userData = eventDetailJson["members"].ToArray().First().ToJson();
                    userJson = JsonConvert.DeserializeObject<JObject>(userData);

                    userImpactedId = userJson["id"].ToString();
                }
                else
                {
                    string content = notificationBodyJson["content"].ToString();
                    requestODataType = content.Substring(3, content.Length - 7);

                    userData = notificationOriginJson["user"].ToJson();
                    userJson = JsonConvert.DeserializeObject<JObject>(userData);
                }

                /* NOTE: The following represent the minimum required data for managing data.
                 * Currently, this is being acquired from the decryptedData as shown below.
                 * These values should be acquired from the source that best suits the developer needs.
                 */
                string chatId = decryptedData["chatId"].ToString();
                string channelId = string.Empty;
                string teamId = string.Empty;
                string tenantId = userJson["tenantId"].ToString();

                if (requestODataType == ChangeNotificationProvider.InstallationEvent || requestODataType == ChangeNotificationProvider.UninstallationEvent)
                {
                    // Manage container lifecycle.
                    await this.ManageContainerLifecycleAsync(requestODataType, chatId, channelId, teamId, tenantId, null).ConfigureAwait(false);
                }
                else if (requestODataType == ChangeNotificationProvider.MemberAddedEvent || requestODataType == ChangeNotificationProvider.MemberDeletedEvent)
                {
                    // Manage permissions.
                    await this.ManageContainerPermissionsAsync(requestODataType, chatId, channelId, teamId, tenantId, userImpactedId).ConfigureAwait(false);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to process chat subscription. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes the subscription from the webhook.
        /// </summary>
        /// <param name="pagedNotificationPayload">Represents the subscription payload from the resource.</param>
        /// <exception cref="Exception">Represents an unexpected error.</exception>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessTeamsChangeNotificationAsync(PagedNotificationPayload pagedNotificationPayload)
        {
            try
            {
                // Decrypt potentially encrypted data.
                JObject decryptedData = this.DecryptNotificationPayload(pagedNotificationPayload);

                // Extract necessary information and deserialize it for ease of access later.
                string eventDetail = decryptedData["eventDetail"].ToJson();
                var channelIdentity = decryptedData["channelIdentity"].ToJson();
                JObject channelIdentityJson = JsonConvert.DeserializeObject<JObject>(channelIdentity);
                JObject eventDetailJson = JsonConvert.DeserializeObject<JObject>(eventDetail);

                string notificationBody = decryptedData["body"].ToJson();
                JObject notificationBodyJson = JsonConvert.DeserializeObject<JObject>(notificationBody);

                /* TODO: Currently, the subscription flow is incomplete.
                 * Feature request is out to support receiving subscriptions.
                 *
                 * This value acquires the subscription event type; it represents if installation or uninstallation
                 * of the application occurs. Typically, it would be acquired from a field within the decryptedData
                 * variable. However, due to the incomplete subscription feature, this field is being acquired from
                 * the message body sent in chat.
                 *
                 * Refer to the README.md for more details.
                 */
                string requestODataType = string.Empty;
                string memberImpactedId = string.Empty;

                if (eventDetailJson != null)
                {
                    // When updates are done to team membership or an app is installed/uninstalled.
                    requestODataType = eventDetailJson["@odata.type"].ToString();
                    var memberData = eventDetailJson["members"].ToArray().First().ToJson();
                    JObject memberJson = JsonConvert.DeserializeObject<JObject>(memberData);
                    memberImpactedId = memberJson["id"].ToString();
                }
                else
                {
                    string content = notificationBodyJson["content"].ToString();
                    requestODataType = content.Substring(3, content.Length - 7);
                }

                /* NOTE: The following represent the minimum required data for managing data.
                 * Currently, this is being acquired from the decryptedData as shown below.
                 * These values should be acquired from the source that best suits the developer needs.
                 */
                string chatId = decryptedData["chatId"].ToString();
                string channelId = channelIdentityJson["channelId"].ToString();
                string teamId = channelIdentityJson["teamId"].ToString();
                string tenantId = channelIdentityJson["tenantId"].ToString();

                if (requestODataType == ChangeNotificationProvider.InstallationEvent || requestODataType == ChangeNotificationProvider.UninstallationEvent)
                {
                    // Manage container lifecycle.
                    await this.ManageContainerLifecycleAsync(requestODataType, chatId, channelId, teamId, tenantId, null).ConfigureAwait(false);
                }
                else if (requestODataType == ChangeNotificationProvider.MemberAddedEvent || requestODataType == ChangeNotificationProvider.MemberDeletedEvent)
                {
                    // Manage permissions.
                    await this.ManageContainerPermissionsAsync(requestODataType, chatId, channelId, teamId, tenantId, memberImpactedId).ConfigureAwait(false);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to process teams subscription. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes and decrypts notification payload.
        /// </summary>
        /// <param name="pagedNotificationPayload">Represents the notification payload.</param>
        /// <returns>The decypted notification payload.</returns>
        private JObject DecryptNotificationPayload(PagedNotificationPayload pagedNotificationPayload)
        {
            JObject decryptedData = null;

            if (pagedNotificationPayload?.Value != null && pagedNotificationPayload.Value.Any())
            {
                // Teams only sends 1 notification per call.
                NotificationPayload notificationPayload = pagedNotificationPayload.Value.First();

                if (notificationPayload.EncryptedContent?.Data != null)
                {
                    // Decrypt encrypted data.
                    X509Certificate2 cert = new X509Certificate2(ChangeNotificationProvider.CertificatePath);
                    string privateKeyText = System.IO.File.ReadAllText(ChangeNotificationProvider.CertificatePrivateKeyPath);

                    RSA privateKeyRSA = RSA.Create();
                    privateKeyRSA.ImportFromPem(privateKeyText);

                    X509Certificate2 certWithPrivateKey = cert.CopyWithPrivateKey(privateKeyRSA);

                    try
                    {
                        // Initialize with the private key that matches the encryptionCertificateId.
                        using var privateKey = certWithPrivateKey.GetRSAPrivateKey();
                        byte[] decryptedSymmetricKey = privateKey.Decrypt(Convert.FromBase64String(notificationPayload.EncryptedContent.DataKey), RSAEncryptionPadding.OaepSHA1);

                        // Can now use decryptedSymmetricKey with the AES algorithm.
                        byte[] encryptedPayload = Convert.FromBase64String(notificationPayload.EncryptedContent.Data);
                        byte[] expectedSignature = Convert.FromBase64String(notificationPayload.EncryptedContent.DataSignature);
                        byte[] actualSignature;

                        using (HMACSHA256 hmac = new HMACSHA256(decryptedSymmetricKey))
                        {
                            actualSignature = hmac.ComputeHash(encryptedPayload);
                        }

                        if (actualSignature.SequenceEqual(expectedSignature))
                        {
                            // Continue with decryption of the encryptedPayload.
                            using AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider
                            {
                                Key = decryptedSymmetricKey,
                                Padding = PaddingMode.PKCS7,
                                Mode = CipherMode.CBC,
                            };

                            // Obtain the initialization vector from the symmetric key itself.
                            int vectorSize = 16;
                            byte[] iv = new byte[vectorSize];
                            Array.Copy(decryptedSymmetricKey, iv, vectorSize);
                            aesProvider.IV = iv;

                            // Decrypt the resource data content.
                            using var decryptor = aesProvider.CreateDecryptor();
                            using MemoryStream msDecrypt = new MemoryStream(encryptedPayload);
                            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                            using StreamReader srDecrypt = new StreamReader(csDecrypt);
                            var decoded = srDecrypt.ReadToEnd();

                            JObject decryptedDataJson = JsonConvert.DeserializeObject<JObject>(decoded);

                            decryptedData = decryptedDataJson;
                        }
                        else
                        {
                            // Log alert. Do not attempt to decrypt encryptedPayload. Assume notification payload has been tampered with and investigate.
                            throw new CryptographicException("Notification payload has been tampered. Please investigate.");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unable to decrypt notification payload. Reason: {ex.Message}");
                    }
                }
                else
                {
                    // Endpoint must always contain encrypted data due to the resource nature.
                    throw new Exception("Unable to process notification payload. Reason: No encrypted data.");
                }
            }

            return decryptedData;
        }

        /// <summary>
        /// Manages the lifecycle of containers depending on installation and installation events in all scopes.
        /// </summary>
        /// <param name="requestODataType">Represents the event type.</param>
        /// <param name="chatId">Represents the chat id of the client.</param>
        /// <param name="channelId">Represents the channel id of the client.</param>
        /// <param name="groupId">Represents the team id of the client.</param>
        /// <param name="tenantId">Represents the tenant id of the client.</param>
        /// <returns>A Task representing the asynchronous completion of the events.</returns>
        private async Task ManageContainerLifecycleAsync(string requestODataType, string chatId, string channelId, string groupId, string tenantId, string userId)
        {
            try
            {
                // Acquire access token.
                string graphAccessToken = await this.authProvider.GetGraphAccessTokenAsync().ConfigureAwait(false);

                // Use scope details to determine pre-calculated containerId prefix.
                string teamsAppInstallationScopeId = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScopeId(chatId, channelId, groupId, tenantId, userId);

                string containerId = this.containerProvider.GetContainerIdFromTeamsAppInstallationScopeId(teamsAppInstallationScopeId);

                // Container Management Conditional.
                switch (requestODataType)
                {
                    case ChangeNotificationProvider.InstallationEvent:
                        // Call Subcriber to Create Container.
                        Container createdContainerModel = await this.containerProvider.CreateOrGetContainerAsync(graphAccessToken, containerId, teamsAppInstallationScopeId).ConfigureAwait(false);

                        if (createdContainerModel.status == "inactive")
                        {
                            // Activate container to prep it for use.
                            await this.containerProvider.ActivateContainerAsync(graphAccessToken, createdContainerModel.id).ConfigureAwait(false);
                        }

                        TeamsAppInstallationScope teamsAppInstallationScope = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScope(chatId, channelId, groupId, userId);

                        // Determine Chat Scope or Team Scope
                        if (teamsAppInstallationScope == TeamsAppInstallationScope.TeamScope)
                        {
                            GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(graphAccessToken);

                            // Acquire Members of Mentioned Scope
                            IChannelMembersCollectionPage result = await graphServiceClient.Teams[groupId].Channels[channelId].Members.Request().GetAsync().ConfigureAwait(false);

                            // Create and Post Permissions
                            foreach (AadUserConversationMember member in result)
                            {
                                List<string> roleList = new List<string>
                                {
                                    "reader",
                                };

                                User user = new User
                                {
                                    userPrincipalName = member.Email,
                                };

                                PermissionUser permissionUser = new PermissionUser
                                {
                                    user = user,
                                };

                                ContainerPermission permissionModel = new ContainerPermission
                                {
                                    roles = roleList,
                                    grantedToV2 = permissionUser,
                                };

                                ContainerPermission createdPermissionModels = await this.containerPermissionProvider.CreateContainerPermissionAsync(graphAccessToken, createdContainerModel.id, permissionModel).ConfigureAwait(false);
                            }
                        }
                        else if (teamsAppInstallationScope == TeamsAppInstallationScope.ChatScope)
                        {
                            GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(graphAccessToken);

                            // Acquire Members of Mentioned Scope
                            IChatMembersCollectionPage result = await graphServiceClient.Chats[chatId].Members.Request().GetAsync().ConfigureAwait(false);

                            // Create and Post Permissions
                            foreach (AadUserConversationMember member in result)
                            {
                                List<string> roleList = new List<string>
                                {
                                    "reader",
                                };

                                User user = new User
                                {
                                    userPrincipalName = member.Email,
                                };

                                PermissionUser permissionUser = new PermissionUser
                                {
                                    user = user,
                                };

                                ContainerPermission permissionModel = new ContainerPermission
                                {
                                    roles = roleList,
                                    grantedToV2 = permissionUser,
                                };

                                ContainerPermission createdPermissionModels = await this.containerPermissionProvider.CreateContainerPermissionAsync(graphAccessToken, createdContainerModel.id, permissionModel).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            throw new Exception("Adding Permissions for User Scope is unsupported.");
                        }
                        break;
                    case ChangeNotificationProvider.UninstallationEvent:
                        // Delete container associated with team. This deletes driveItems, permissions and everything else associated with the container.
                        if (containerId != null)
                        {
                            await this.containerProvider.DeleteContainerAsync(graphAccessToken, containerId, teamsAppInstallationScopeId).ConfigureAwait(false);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to manage container lifecycle. Reason: {ex.Message}");
            }
        }

        private async Task ManageContainerPermissionsAsync(string requestODataType, string chatId, string channelId, string groupId, string tenantId, string userId)
        {
            try
            {
                // Acquire access token.
                string graphAccessToken = await this.authProvider.GetGraphAccessTokenAsync().ConfigureAwait(false);

                // Use scope details to determine pre-calculated containerId prefix.
                string teamsAppInstallationScopeId = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScopeId(chatId, channelId, groupId, tenantId, userId);

                // Acquire container if one exists.
                string containerId = this.containerProvider.GetContainerIdFromTeamsAppInstallationScopeId(teamsAppInstallationScopeId);

                // Graph API can sometimes send notifications from the past. Thus, there can be a scenario where a container does not exist. This handles it.
                if (containerId != null)
                {
                    // Read all permissions.
                    IEnumerable<ContainerPermission> containerPermissionModels = await this.containerPermissionProvider.ReadContainerPermissionAsync(graphAccessToken, containerId).ConfigureAwait(false);

                    // Initialize service client.
                    GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(graphAccessToken);

                    switch (requestODataType)
                    {
                        case ChangeNotificationProvider.MemberDeletedEvent:
                            // Get User that was removed.
                            Microsoft.Graph.User userRemoved = await graphServiceClient.Users[userId].Request().GetAsync().ConfigureAwait(false);

                            // Filter permissions to find the ones associated with the removed user.
                            IEnumerable<ContainerPermission> filteredContainerPermissionModels = containerPermissionModels.Where((model) => model.grantedToV2.user.userPrincipalName == userRemoved.UserPrincipalName.ToLower());

                            // Delete all permissions associated with the removed user.
                            foreach (ContainerPermission containerPermissionModel in filteredContainerPermissionModels)
                            {
                                await this.containerPermissionProvider.DeleteContainerPermissionAsync(graphAccessToken, containerId, containerPermissionModel.id).ConfigureAwait(false);
                            }

                            break;
                        case ChangeNotificationProvider.MemberAddedEvent:
                            // Get User that was removed.
                            Microsoft.Graph.User userAdded = await graphServiceClient.Users[userId].Request().GetAsync().ConfigureAwait(false);

                            List<string> roleList = new List<string>
                                    {
                                        "reader",
                                    };

                            User user = new User
                            {
                                userPrincipalName = userAdded.UserPrincipalName,
                            };

                            PermissionUser permissionUser = new PermissionUser
                            {
                                user = user,
                            };

                            ContainerPermission addedUserPermissionModel = new ContainerPermission
                            {
                                roles = roleList,
                                grantedToV2 = permissionUser,
                            };

                            await this.containerPermissionProvider.CreateContainerPermissionAsync(graphAccessToken, containerId, addedUserPermissionModel).ConfigureAwait(false);

                            break;
                    }
                }
                else
                {
                    throw new Exception("Unable to modify permissions. Reason: Container does not exist.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to manage container permissions. Reason: {ex.Message}");
            }
        }
    }
}
