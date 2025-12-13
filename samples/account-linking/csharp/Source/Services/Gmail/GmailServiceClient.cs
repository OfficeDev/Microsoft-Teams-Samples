using Microsoft.Extensions.Options;
using Microsoft.Teams.Samples.AccountLinking.Sample.Services.OAuth;
using System.Net.Http.Headers;

namespace Microsoft.Teams.Samples.AccountLinking.SampleClient.Services.Gmail
{
    public class GmailServiceClient
    {
        private readonly ILogger<GmailServiceClient> _logger;

        private readonly HttpClient _httpClient;

        private readonly ExternalAuthParameters _externalAuthParameters;

        public GmailServiceClient(
            ILogger<GmailServiceClient> logger,
            HttpClient httpClient,
            IOptions<ExternalAuthParameters> externalAuthParameters)
        {
            _logger = logger;
            _httpClient = httpClient;
            _externalAuthParameters = externalAuthParameters.Value;
        }

        public async Task<GmailUserProfile> GetUserProfile(string userAccessToken)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://gmail.googleapis.com/gmail/v1/users/{_externalAuthParameters.GmailAddress}/profile"),
                    Method = HttpMethod.Get
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);
                var response = await _httpClient.SendAsync(request);
                _logger.LogInformation("Result: {code}", response.StatusCode);

                response.EnsureSuccessStatusCode();
                var content1 = await response.Content.ReadAsStringAsync();

                var content = await response.Content.ReadFromJsonAsync<GmailUserProfile>();
                return content ?? throw new HttpRequestException("Failed to fetch gmail user profile");
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
