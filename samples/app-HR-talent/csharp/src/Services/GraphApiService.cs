using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Services.Interfaces;
using TeamsTalentMgmtApp.Models.DatabaseContext;
using TeamsTalentMgmtApp.Services.Data;
using Microsoft.Graph.Models;
using Azure.Core;

namespace TeamsTalentMgmtApp.Services
{
    public class GraphApiService : IGraphApiService
    {
        private readonly AppSettings _appSettings;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<GraphApiService> _logger;
        private readonly IConfiguration _configuration;

        public GraphApiService(
            ILogger<GraphApiService> logger,
            IOptions<AppSettings> appSettings,
            IConfiguration configuration,
            DatabaseContext databaseContext)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _databaseContext = databaseContext;
        }

        public async Task<(string upn, string chatId)> GetProactiveChatIdForUser(string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken)
        {
            var token = await GetTokenForApp(tenantId);

            return await GetProactiveChatIdForUserInternal(token, aliasUpnOrOid, tenantId, cancellationToken);
        }

        private async Task<(string upn, string chatId)> GetProactiveChatIdForUserInternal(string token, string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken)
        {
            var graphClient = GetGraphServiceClient(token);

            var upn = await GetUpnFromAlias(token, aliasUpnOrOid, cancellationToken);
            if (upn == null)
            {
                return (null, null);
            }

            var installedApps = await graphClient.Users[upn].Teamwork.InstalledApps.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"teamsApp/externalId eq '{_configuration["TeamsAppId"]}'";
                config.QueryParameters.Expand = new[] { "teamsApp" };
            }, cancellationToken);

            var app = installedApps?.Value?.FirstOrDefault();
            if (app == null)
            {
                return (upn, null);
            }

            var chat = await graphClient.Users[upn].Teamwork.InstalledApps[app.Id].Chat.GetAsync(cancellationToken: cancellationToken);
            return (upn, chat.Id);
        }

        private async Task<string> GetUpnFromAlias(string token, string aliasUpnOrOid, CancellationToken cancellationToken)
        {
            if (aliasUpnOrOid.Contains('@'))
            {
                return aliasUpnOrOid;
            }

            if (Guid.TryParse(aliasUpnOrOid, out _))
            {
                return aliasUpnOrOid;
            }

            var graphClient = GetGraphServiceClient(token);

            var users = await graphClient.Users.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"startswith(userPrincipalName,'{aliasUpnOrOid}@')";
            }, cancellationToken);

            var user = users?.Value?.FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            return user.UserPrincipalName;
        }

        public async Task<InstallResult> InstallBotForUser(string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken)
        {
            var token = await GetTokenForApp(tenantId);
            var upn = await GetUpnFromAlias(token, aliasUpnOrOid, cancellationToken);

            if (upn == null)
            {
                return InstallResult.AliasNotFound;
            }

            var graphClient = GetGraphServiceClient(token);

            var teamsApps = await graphClient.AppCatalogs.TeamsApps.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"distributionMethod eq 'organization' and externalId eq '{_appSettings.TeamsAppId}'";
            }, cancellationToken);

            var teamApp = teamsApps?.Value?.FirstOrDefault();
            var success = false;
            if (!string.IsNullOrEmpty(teamApp?.Id))
            {
                try
                {

                    var requestBody = new Microsoft.Graph.Models.UserScopeTeamsAppInstallation
                    {
                        TeamsApp = new Microsoft.Graph.Models.TeamsApp
                        {
                            Id = _configuration["TeamsAppId"]
                        }
                    };

                    var installBotRequest = await graphClient.Users[upn].Teamwork.InstalledApps.PostAsync(requestBody);


                    var Body = new UserScopeTeamsAppInstallation
                    {
                        AdditionalData = new Dictionary<string, object>
                        {
                            {
                                "teamsApp@odata.bind",
                                $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{_configuration["TeamsAppId"]}"
                            }
                        }
                    };

                    await graphClient.Users[upn].Teamwork.InstalledApps.PostAsync(Body,cancellationToken: cancellationToken);

                    // Getting the chat id will confirm the installation was successful and send the welcome message
                    await GetProactiveChatIdForUserInternal(token, aliasUpnOrOid, tenantId, cancellationToken);

                    success = true;
                }
                catch (ServiceException svcEx) when ((int?)svcEx.ResponseStatusCode == 409)
                {
                    // 409 Conflict: Bot is already installed
                    success = true;
                }
                catch (Exception ex)
                {
                    // We don't want to show any exception for user in this case.
                    success = false;
                    _logger.LogError(ex, ex.Message);
                }
            }

            return success ? InstallResult.InstallSuccess : InstallResult.InstallFailed;
        }

        public async Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken)
        {
            var graphClient = GetGraphServiceClient(token);

            // If you have a user's UPN, you can add it directly to a group, but then there will be a
            // significant delay before Microsoft Teams reflects the change. Instead, we find the user
            // object's id, and add the ID to the group through the Graph beta endpoint, which is
            // recognized by Microsoft Teams much more quickly. See
            // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/teams_api_overview
            // for more about delays with adding members.
            var requester = await graphClient.Me.GetAsync(cancellationToken: cancellationToken);

            var createdGroup = await graphClient.Groups.PostAsync(
                new Group
                {
                    DisplayName = $"Position {position.PositionExternalId}",
                    MailNickname = position.PositionExternalId,
                    Description = $"Everything about position {position.PositionExternalId}",
                    Visibility = "Private",
                    GroupTypes = new List<string> { "Unified" }, 
                    MailEnabled = true,
                    SecurityEnabled = false,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "members@odata.bind", await GetTeamMemberIds(graphClient, position, requester, cancellationToken) },
                        { "owners@odata.bind", await GetTeamOwnerIds(graphClient, position, requester, cancellationToken) }
                    }
                },
                cancellationToken: cancellationToken);


            var team = await graphClient.Groups[createdGroup.Id].Team.PutAsync(
                new Team
                {
                    GuestSettings = new TeamGuestSettings
                    {
                        AllowCreateUpdateChannels = false,
                        AllowDeleteChannels = false
                    },
                    MemberSettings = new TeamMemberSettings
                    {
                        AllowCreateUpdateChannels = true
                    },
                    MessagingSettings = new TeamMessagingSettings
                    {
                        AllowUserEditMessages = true,
                        AllowUserDeleteMessages = true
                    },
                    FunSettings = new TeamFunSettings
                    {
                        AllowGiphy = true,
                        GiphyContentRating = GiphyRatingType.Strict
                    }
                }, cancellationToken: cancellationToken);

            var channel = await graphClient.Teams[team.Id].Channels.PostAsync(
                new Channel
                {
                    DisplayName = "Candidates",
                    Description = "Discussion about interview, feedback, etc."
                },  cancellationToken: cancellationToken);


            var teamsApps = await graphClient.AppCatalogs.TeamsApps.GetAsync(
                    requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Filter =
                            $"distributionMethod eq 'organization' and externalId eq '{_appSettings.TeamsAppId}'";
                    },
                    cancellationToken
                  );


            var teamApp = teamsApps.Value.FirstOrDefault();
            if (!string.IsNullOrEmpty(teamApp?.Id))
            {
                var appId = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamApp.Id}";
                await graphClient.Teams[team.Id].InstalledApps.PostAsync(
                        new TeamsAppInstallation
                        {
                            AdditionalData = new Dictionary<string, object>
                            {
                                { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{appId}" }
                            }
                        },
                        cancellationToken: cancellationToken
                );

                var contentUrl = $"{_appSettings.BaseUrl}/StaticViews/TeamTab.html?positionId={position.PositionId}";
                await graphClient.Teams[team.Id].Channels[channel.Id].Tabs.PostAsync(
                    new TeamsTab
                    {
                        DisplayName = position.PositionExternalId,
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{appId}" }
                        },
                        Configuration = new TeamsTabConfiguration
                        {
                            EntityId = position.PositionId.ToString(),
                            ContentUrl = contentUrl,
                            WebsiteUrl = contentUrl + "&web=1"
                        }
                    },
                    cancellationToken: cancellationToken
                );

            }

            return (team, createdGroup.DisplayName);
        }

        private async Task<string[]> GetTeamMemberIds(
            GraphServiceClient graphClient,
            Position position,
            User requester,
            CancellationToken cancellationToken)
        {
            var result = new HashSet<string>
            {
                requester.Id
            };

            var hiringManager = position.HiringManager;
            if (hiringManager != null && !string.IsNullOrEmpty(hiringManager.DirectReportIds))
            {
                var ids = hiringManager.DirectReportIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x));

                var members = await _databaseContext.Recruiters
                    .Where(x => ids.Contains(x.RecruiterId))
                    .ToArrayAsync(cancellationToken);

                // because of demo, we don't know user upn and have to build on the flight
                var domain = new MailAddress(requester.UserPrincipalName).Host;
                foreach (var member in members)
                {
                    var upn = $"{member.Alias}@{domain}";

                    var usersPage = await graphClient.Users.GetAsync(config =>
                    {
                        config.QueryParameters.Filter = $"userPrincipalName eq '{upn}'";
                    }, cancellationToken);

                    if (usersPage?.Value?.Count == 1)
                    {
                        result.Add(usersPage.Value[0].Id);
                    }
                }

            }

            return result.Select(CovertIdToOdataResourceFormat).ToArray();
        }

        private static async Task<string[]> GetTeamOwnerIds(
            GraphServiceClient graphClient,
            Position position,
            User requester,
            CancellationToken cancellationToken)
        {
            var owners = new HashSet<string>
            {
                requester.Id
            };

            var hiringManager = position.HiringManager;
            if (hiringManager != null)
            {
                // because of demo, we don't know user upn and have to build on the flight
                var domain = new MailAddress(requester.UserPrincipalName).Host;
                var upn = $"{hiringManager.Alias}@{domain}";
                var users = await graphClient.Users.GetAsync(config =>
                {
                    config.QueryParameters.Filter = $"userPrincipalName eq '{upn}'";
                }, cancellationToken);

                var user = users?.Value?.FirstOrDefault();
                if (user != null)
                {
                    owners.Add(user.Id);
                }
            }

            return owners.Select(CovertIdToOdataResourceFormat).ToArray();
        }

        private static string CovertIdToOdataResourceFormat(string id) => $"https://graph.microsoft.com/v1.0/users/{id}";

        public class SimpleTokenCredential : TokenCredential
        {
            private readonly string _token;

            public SimpleTokenCredential(string token)
            {
                _token = token;
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new AccessToken(_token, DateTimeOffset.Now.AddHours(1));

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.Now.AddHours(1)));
        }

        private GraphServiceClient GetGraphServiceClient(string token)
        {
            var credential = new SimpleTokenCredential(token);

            return new GraphServiceClient(credential);
        }

        private async Task<string> GetTokenForApp(string tenantId)
        {
            var builder = ConfidentialClientApplicationBuilder.Create(_configuration["MicrosoftAppId"])
                .WithClientSecret(_configuration["MicrosoftAppPassword"])
                .WithTenantId(tenantId)
                .WithRedirectUri("msal" + _configuration["MicrosoftAppId"] + "://auth");

            var client = builder.Build();

            var tokenBuilder = client.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" });

            var result = await tokenBuilder.ExecuteAsync();

            return result.AccessToken;
        }
    }
}
