using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using TypeaheadSearch.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TypeaheadSearch.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Controller(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received");
            
            var text = activity.Text?.Trim().ToLower();
            
            if (text == "staticsearch")
            {
                await SendAdaptiveCard(client, "Cards/StaticSearchCard.json");
            }
            else if (text == "dynamicsearch")
            {
                await SendAdaptiveCard(client, "Cards/DynamicSearchCard.json");
            }
            else if (text == "dependantdropdown")
            {
                await SendAdaptiveCard(client, "Cards/DependentDropdown.json");
            }
            else if (!string.IsNullOrEmpty(activity.Value?.ToString()))
            {
                // Handle card submit
                await HandleCardSubmit(activity, client, log);
            }
            else
            {
                await client.Send($"You said '{activity.Text}'. Try 'staticsearch', 'dynamicsearch', or 'dependantdropdown'");
            }
        }

        private async Task SendAdaptiveCard(IContext.Client client, string cardPath)
        {
            var cardJson = File.ReadAllText(cardPath);
            var cardContent = JsonSerializer.Deserialize<JsonElement>(cardJson);
            
            // Create a dictionary-based activity structure
            var activity = new Dictionary<string, object>
            {
                ["type"] = "message",
                ["attachments"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["contentType"] = "application/vnd.microsoft.card.adaptive",
                        ["content"] = cardContent
                    }
                }
            };
            
            // Serialize to JSON and send as raw activity
            var activityJson = JsonSerializer.Serialize(activity);
            var activityObject = JsonSerializer.Deserialize<MessageActivity>(activityJson);
            
            await client.Send(activityObject);
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "How can I help you today?";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }

        [Activity("application/search")]
        public async Task<object> HandleSearchInvoke([Context] InvokeActivity activity, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                log.Info("Search invoke received");
                
                // Parse the value as a JSON object to handle dynamic structure
                var valueJson = activity.Value?.ToString();
                if (string.IsNullOrEmpty(valueJson))
                {
                    return new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = new object[0] } };
                }

                var valueNode = JsonNode.Parse(valueJson);
                var dataset = valueNode?["dataset"]?.ToString();
                var queryText = valueNode?["queryText"]?.ToString() ?? string.Empty;

                log.Info($"Search invoke - dataset: {dataset}, queryText: {queryText}");

                if (dataset == "npmpackages")
                {
                    return await HandleNpmPackageSearch(queryText);
                }
                else if (dataset == "cities")
                {
                    // Get the data object which contains associated inputs
                    var dataNode = valueNode?["data"];
                    string country = string.Empty;
                    
                    if (dataNode != null)
                    {
                        // The country input value is in the data object
                        var countryValue = dataNode["choiceSelect"];
                        if (countryValue != null)
                        {
                            country = countryValue.ToString().ToLower();
                        }
                    }
                    
                    log.Info($"Cities search - country: {country}");
                    return GetCountrySpecificData(country);
                }

                return new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = new object[0] } };
            }
            catch (Exception ex)
            {
                log.Error($"Error in search invoke: {ex.Message}");
                return new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = new object[0] } };
            }
        }

        private async Task<object> HandleNpmPackageSearch(string queryText)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var packageResult = await httpClient.GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{queryText}&prerelease=true");
            
            var packageJson = JsonNode.Parse(packageResult);
            var data = packageJson?["data"]?.AsArray();
            
            var packages = new List<object>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    var id = item?["id"]?.ToString() ?? string.Empty;
                    var description = item?["description"]?.ToString() ?? string.Empty;
                    packages.Add(new { title = id, value = $"{id} - {description}" });
                }
            }

            return new
            {
                type = "application/vnd.microsoft.search.searchResponse",
                value = new { results = packages }
            };
        }

        private async Task HandleCardSubmit(MessageActivity activity, IContext.Client client, Microsoft.Teams.Common.Logging.ILogger log)
        {
            try
            {
                var valueJson = activity.Value?.ToString();
                if (string.IsNullOrEmpty(valueJson))
                {
                    await client.Send("No data submitted");
                    return;
                }

                var valueNode = JsonNode.Parse(valueJson);
                var choiceSelect = valueNode?["choiceSelect"]?.ToString();
                var queryText = valueNode?["queryText"]?.ToString();
                var city = valueNode?["city"]?.ToString();

                string responseMessage;
                if (!string.IsNullOrEmpty(choiceSelect) && !string.IsNullOrEmpty(city))
                {
                    responseMessage = $"You selected country: {choiceSelect} and city: {city}";
                }
                else if (!string.IsNullOrEmpty(choiceSelect))
                {
                    responseMessage = $"You selected: {choiceSelect}";
                }
                else if (!string.IsNullOrEmpty(queryText))
                {
                    responseMessage = $"You searched for: {queryText}";
                }
                else
                {
                    responseMessage = "Form submitted successfully!";
                }

                await client.Send(responseMessage);
            }
            catch (Exception ex)
            {
                log.Error($"Error handling card submit: {ex.Message}");
                await client.Send("Error processing your submission");
            }
        }

        private object GetCountrySpecificData(string country)
        {
            var usa = new[]
            {
                new { title = "CA", value = "CA" },
                new { title = "FL", value = "FL" },
                new { title = "TX", value = "TX" }
            };

            var france = new[]
            {
                new { title = "Paris", value = "Paris" },
                new { title = "Lyon", value = "Lyon" },
                new { title = "Nice", value = "Nice" }
            };

            var india = new[]
            {
                new { title = "Delhi", value = "Delhi" },
                new { title = "Mumbai", value = "Mumbai" },
                new { title = "Pune", value = "Pune" }
            };

            return country switch
            {
                "usa" => new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = usa } },
                "france" => new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = france } },
                _ => new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = india } }
            };
        }
    }
}