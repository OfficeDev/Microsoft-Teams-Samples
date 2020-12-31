using Bogus;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
                Guid = Guid.NewGuid().ToString()
            };
        }

        public static async Task PostTaskNotification(string webhook, TaskItem item, string title)
        {
            string cardJson = GetConnectorCardJson(item, title);
            await PostCardAsync(webhook, cardJson);
        }

        public static async Task PostWelcomeMessage(string webhookUrl)
        {
            string cardJson = @"{
            ""@type"": ""MessageCard"",
            ""summary"": ""Welcome Message"",
            ""sections"": [{ 
                ""activityTitle"": ""Welcome Message"",
                ""text"": ""Teams ToDo connector has been set up. We will send you notification whenever new task is added in [Task Manager Portal]("+AppSettings.BaseUrl+ @" ).""}]}";

            await PostCardAsync(webhookUrl, cardJson);
        }

        private static async Task PostCardAsync(string webhook, string cardJson)
        {
            //prepare the http POST
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StringContent(cardJson, System.Text.Encoding.UTF8, "application/json");
            using (var response = await client.PostAsync(webhook, content))
            {
                // Check response.IsSuccessStatusCode and take appropriate action if needed.
            }
        }

        public static string GetConnectorCardJson(TaskItem task, string title)
        {
            //prepare the json payload
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
                                '" + AppSettings.BaseUrl + "/task/detail/" + task.Guid + @"'
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
                              'target': '" + AppSettings.BaseUrl + "/task/update?id=" + task.Guid + @"',
                              'body': '{""Title"":""{{title.value}}""}',
                                'bodyContentType': 'application/json'
                            }
                          ]
                        }
                    ]}";
        }
    }
}