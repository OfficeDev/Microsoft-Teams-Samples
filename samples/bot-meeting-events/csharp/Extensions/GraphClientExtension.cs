using Microsoft.Graph;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Communications.Common.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingEventsCallingBot.Extenstions
{
    public static class GraphClientExtension
    {
        public static Task<IGraphResponse<TResponse>> SendAsync<TRequest, TResponse>(
            this IGraphClient client,
            IGraphRequest<TRequest> request,
            string tenant,
            Guid scenarioId,
            CancellationToken cancellationToken = default(CancellationToken))
            where TRequest : class
            where TResponse : class
        {
            if (!string.IsNullOrWhiteSpace(tenant))
            {
                request.Properties.Add(GraphProperty.Property(HttpConstants.HeaderNames.Tenant, tenant));
            }

            request.Properties.Add(GraphProperty.RequestProperty(HttpConstants.HeaderNames.ScenarioId, scenarioId));
            request.Properties.Add(GraphProperty.RequestProperty(HttpConstants.HeaderNames.ClientRequestId, Guid.NewGuid()));

            return client.SendAsync<TRequest, TResponse>(request, cancellationToken);
        }

        public static async Task<T> SendAsync<T>(
            this IGraphClient client,
            IBaseRequest request,
            RequestType requestType,
            string tenant,
            Guid scenarioId,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            try
            {
                var graphRequest = CreateGraphRequest(request, requestType);
                var graphResponse = await client
                    .SendAsync<object, T>(graphRequest, tenant, scenarioId, cancellationToken)
                    .ConfigureAwait(false);

                return graphResponse.Content;
            }
            catch
            {
                return null;
            }
        }

        public static Task SendAsync(
            this IGraphClient client,
            IBaseRequest request,
            RequestType requestType,
            string tenant,
            Guid scenarioId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.SendAsync<NoContentMessage>(request, requestType, tenant, scenarioId, cancellationToken);
        }


        private static IGraphRequest<object> CreateGraphRequest(
            IBaseRequest request,
            RequestType requestType)
        {
            const string requestBodyName = "RequestBody";
            var requestObject = request
                .NotNull(nameof(request))
                .GetPropertyUsingReflection(requestBodyName);

            var requestUri = request.GetHttpRequestMessage().RequestUri;

            return new GraphRequest<object>(requestUri, requestObject, requestType);
        }
    }
}
