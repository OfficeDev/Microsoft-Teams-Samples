using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace MeetingAutoRecording.Helper
{
    public class GraphClient
    {
        public static GraphServiceClient GetGraphClient(string accessToken)
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                return Task.CompletedTask;
            }))
            {
                BaseUrl = "https://graph.microsoft.com/beta"
            };
            return graphClient;
        }
    }
}