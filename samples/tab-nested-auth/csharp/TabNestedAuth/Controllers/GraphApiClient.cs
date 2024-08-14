using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class GraphApiClient
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<string> CallGraphApiAsync(string accessToken)
    {
        var graphEndpoint = "https://graph.microsoft.com/v1.0/me";
        var request = new HttpRequestMessage(HttpMethod.Get, graphEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            // Return the response body as a string
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        else
        {
            // Return an error message
            return $"Error: {response.StatusCode}";
        }
    }
}

