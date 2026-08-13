using Bogus;
using System.Net.Http.Headers;
using TeamsToDoAppConnector.Models;

namespace TeamsToDoAppConnector.Utils
{
    public static class TaskHelper
    {
        public static TaskItem CreateTaskItem()
        {
            var faker = new Faker();
            return new TaskItem()
            {
                Title = faker.Commerce.ProductName(),
                Description = faker.Lorem.Sentence(),
                Assigned = $"{faker.Name.FirstName()} {faker.Name.LastName()}",
                Guid = System.Guid.NewGuid().ToString()
            };
        }

        public static async Task PostTaskNotification(string webhook, TaskItem item, string title, string baseUrl, string[]? allowedHostSuffixes)
        {
            string cardJson = GetConnectorCardJson(item, title, baseUrl);
            await PostCardAsync(webhook, cardJson, allowedHostSuffixes);
        }

        public static async Task PostWelcomeMessage(string webhookUrl, string baseUrl, string[]? allowedHostSuffixes)
        {
            string cardJson = @"{
            ""@type"": ""MessageCard"",
            ""summary"": ""Welcome Message"",
            ""sections"": [{ 
                ""activityTitle"": ""Welcome Message"",
                ""text"": ""Teams ToDo connector has been set up. We will send you notification whenever new task is added in [Task Manager Portal](" + baseUrl + "/task/create" + @").""}]}";

            await PostCardAsync(webhookUrl, cardJson, allowedHostSuffixes);
        }

        private static async Task PostCardAsync(string webhook, string cardJson, string[]? allowedHostSuffixes)
        {
            // Re-validate the destination immediately before the outbound call (defense in depth against SSRF).
            if (!WebhookValidator.IsValid(webhook, allowedHostSuffixes, out _))
            {
                return;
            }

            // Disable automatic redirects so a trusted host cannot bounce the request to an internal target.
            using var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StringContent(cardJson, System.Text.Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(webhook, content);
        }

        public static string GetConnectorCardJson(TaskItem task, string title, string baseUrl)
        {
            return @"
                {
                    'summary': 'A task is added.',
                    'sections': [
                        {
                            'activityTitle': 'Task "+ title + @"!',
                            'facts': [
                                {
                                    'name': 'Title:',
                                    'value': '" + task.Title + @"'
                                },
                                {
                                    'name': 'Description:',
                                    'value': '" + task.Description + @"'
                                },
                                {
                                    'name': 'Assigned To:',
                                    'value': '" + task.Assigned + @"'
                                }
                            ]
                        }
                    ],
                    'potentialAction': [
                        {
                            '@context': 'http://schema.org',
                            '@type': 'ViewAction',
                            'name': 'View Task Details',
                            'target': [
                                '" + baseUrl + "/task/detail/" + task.Guid + @"'
                            ]
                        },
                        {
                          '@type': 'ActionCard',
                          'name': 'Update Title',
                          'inputs': [
                            {
                              '@type': 'TextInput',
                              'id': 'title',
                              'isMultiline': true,
                              'title': 'Please enter new title'
                            }
                          ],
                          'actions': [
                            {
                              '@type': 'HttpPOST',
                              'name': 'Update Title',
                              'isPrimary': true,
                              'target': '" + baseUrl + "/task/update?id=" + task.Guid + @"',
                              'body': '{""Title"":""{{title.value}}""}',
                                'bodyContentType': 'application/json'
                            }
                          ]
                        }
                    ]}";
        }
    }
}