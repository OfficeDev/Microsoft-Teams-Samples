// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace ChatLifecycle.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ChatLifecycle.Helper;
    using ChatLifecycle.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly string pollyAppId = "AzureAd:PollyId";
        public static string accessToken;
        //   public static List<AdaptiveResultModel> members = new List<AdaptiveResultModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <param name="httpClientFactory">IHttpClientFactory instance.</param>
        /// <param name="httpContextAccessor">IHttpContextAccessor instance.</param>
        public HomeController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Configure()
        {

            return View();
        }

        [Route("CreateAdaptiveCard")]
        public ActionResult CreateAdaptiveCard()
        {
            return View();
        }

        /// <summary>
        /// Retrieve team members along with profile pictures
        /// </summary>
        /// <returns>Returns Team members details</returns>
        [Authorize]
        [HttpGet("GetUserAccessToken")]
        public async Task<ActionResult<string>> GetUserAccessToken()
        {
            try
            {
                accessToken = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);
                return accessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [Route("CreateNewTeam")]
        public bool CreateNewTeam(string resultJson, string userID, string title)
        {
            string[] members = { };
            if (resultJson.Contains(','))
            {
                members = resultJson.Split(',');
            }
            else
            {
                members = new[] { resultJson };
            }

            var graphClient = GraphClient.GetGraphClient(accessToken);
            CreateGroupChat(graphClient, members, userID, title);
            return true;
        }

        public async void CreateGroupChat(GraphServiceClient graphClient, string[] members, string userID, string title)
        {
            var pollyId = _configuration[pollyAppId];
            var chat = new Chat
            {
                ChatType = ChatType.Group,
                Topic = title,
                Members = new ChatMembersCollectionPage()
                {
                      new AadUserConversationMember
                      {
                           Roles = new List<String>()
                           {
                             "owner"
                           },
                      AdditionalData = new Dictionary<string, object>()
                      {
                        {"user@odata.bind", "https://graph.microsoft.com/v1.0/users('"+members[0]+"')"}
                      }
                      },
                    new AadUserConversationMember
                    {
                       Roles = new List<String>()
                       {
                         "owner"
                       },
                    AdditionalData = new Dictionary<string, object>()
                    {
                         {"user@odata.bind", "https://graph.microsoft.com/v1.0/users('"+userID+"')"}
                    }
                    }
                }
            };


            var response = await graphClient.Chats
                 .Request()
                 .AddAsync(chat);

            if (members.Length == 2)
            {
                AddMemberWithoutHistory(graphClient, response, members);
                DeleteMember(graphClient,response);
            }

            else if (members.Length == 3)
            {
                AddMemberWithHistory(graphClient, response, members);
                AddMemberWithoutHistory(graphClient, response, members);
                DeleteMember(graphClient, response);
            }

            else if (members.Length >= 4)
            {
                AddMemberWithHistory(graphClient, response, members);
                AddMemberWithoutHistory(graphClient, response, members);
                AddMemberWithNoOfDays(graphClient, response, members);
                DeleteMember(graphClient, response);
            }

            //Adding Polly app to chat
            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                   {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/"+pollyId}
                }
            };

            await graphClient.Chats[response.Id].InstalledApps
                .Request()
                .AddAsync(teamsAppInstallation);


            //Adding Polly as tab to chat
            var teamsTab = new TeamsTab
            {
                DisplayName = "Associate Insights",
                Configuration = new TeamsTabConfiguration
                {
                    EntityId = "pollyapp",
                    ContentUrl = "https://teams.polly.ai/msteams/content/meeting/tab?theme={theme}",
                    WebsiteUrl = null,
                    RemoveUrl = "https://teams.polly.ai/msteams/content/tabdelete?theme={theme}"
                },
                AdditionalData = new Dictionary<string, object>()
                {
                  {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/"+pollyId}
                }
            };

            await graphClient.Chats[response.Id].Tabs
                .Request()
                .AddAsync(teamsTab);
        }

        //Adding member with full chat history
        public async void AddMemberWithHistory(GraphServiceClient graphClient, Chat response, string[] members)
        {
            var conversationMember = new AadUserConversationMember
            {
                VisibleHistoryStartDateTime = DateTimeOffset.Parse("0001-01-01T00:00:00Z"),
                Roles = new List<string>() { "owner" },
                AdditionalData = new Dictionary<string, object>()
                {
                   {"user@odata.bind", "https://graph.microsoft.com/v1.0/users/"+members[1]}
                }
            };

            await graphClient.Chats[response.Id].Members
                .Request()
                .AddAsync(conversationMember);
        }

        //Adding member with no chat history
        public async void AddMemberWithoutHistory(GraphServiceClient graphClient, Chat response, string[] members)
        {
            var conversationMember = new AadUserConversationMember
            {
                Roles = new List<string>() { "owner" },
                AdditionalData = new Dictionary<string, object>()
                {
                  {"user@odata.bind", "https://graph.microsoft.com/v1.0/users/"+members[2]}
                }
            };

            await graphClient.Chats[response.Id].Members
                .Request()
                .AddAsync(conversationMember);
        }

        //Adding members with no. of days chat history
        public async void AddMemberWithNoOfDays(GraphServiceClient graphClient, Chat response, string[] members)
        {

            if (members.Length < 4)
            {
                var conversationMember = new AadUserConversationMember
                {
                    VisibleHistoryStartDateTime = DateTime.Now.AddDays(-1),
                    Roles = new List<string>() { "owner" },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"user@odata.bind", "https://graph.microsoft.com/v1.0/users/"+members[3]}
                    }
                };

                await graphClient.Chats[response.Id].Members
                    .Request()
                    .AddAsync(conversationMember);
            }
            else
            {
                List<string> membersWithDays = new List<string>();
                int i = 0;
                foreach (var member in members)
                {
                    if (i > 4)
                    {
                        membersWithDays.Add(member);
                    }
                    i++;
                }
                foreach (var member in membersWithDays)
                {
                    var conversationMember = new AadUserConversationMember
                    {
                        VisibleHistoryStartDateTime = DateTime.Now.AddDays(-1),
                        Roles = new List<string>() { "owner" },
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"user@odata.bind", "https://graph.microsoft.com/v1.0/users/"+member}
                        }
                    };

                    await graphClient.Chats[response.Id].Members
                        .Request()
                        .AddAsync(conversationMember);
                }
            }
        }

        //Delete first member added
        public async void DeleteMember(GraphServiceClient graphClient, Chat response)
        {
            var chat = await graphClient.Chats[response.Id]
                .Request()
                .Expand("members")
                .GetAsync();
            var convMemID = chat.Members.CurrentPage[0].Id;

            await graphClient.Chats[response.Id].Members[convMemID]
                .Request()
                .DeleteAsync();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
