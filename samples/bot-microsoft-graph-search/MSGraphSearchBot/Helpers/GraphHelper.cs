using Microsoft.Extensions.Options;
using Microsoft.Graph;
using MSGraphSearchSample.Interfaces;
using MSGraphSearchSample.Models;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Helpers
{
    public class GraphHelper : IGraphHelper
    {
        protected readonly AppConfigOptions _appconfig;
        public GraphHelper(IOptions<AppConfigOptions> options)
        {
            _appconfig = options.Value;
        }

        public GraphServiceClient GetDelegatedServiceClient(string _token)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));
            return graphClient;
        }

    }
}
