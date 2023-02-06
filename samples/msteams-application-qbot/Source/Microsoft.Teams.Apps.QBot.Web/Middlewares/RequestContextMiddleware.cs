namespace Microsoft.Teams.Apps.QBot.Web.Middlewares
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Request context middleware.
    /// </summary>
    public class RequestContextMiddleware
    {
        private const string RequestIdHeader = "request-id";
        private const string OperationIdHeader = "x-operationId";

        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContextMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next step in the request pipeline.</param>
        public RequestContextMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        /// <returns>Next task in the pipeline.</returns>
        public Task Invoke(HttpContext httpContext)
        {
            // Add client request id to response.
            if (httpContext.Request.Headers.TryGetValue(RequestIdHeader, out var requestIds))
            {
                var requestId = requestIds.FirstOrDefault();
                httpContext.Response.Headers.Add(RequestIdHeader, requestId);
            }

            // Add operation id to response. This will help in tracking Traces.
            httpContext.Response.Headers.Add(OperationIdHeader, Activity.Current.TraceId.ToString());

            // Next operation.
            return this.next.Invoke(httpContext);
        }
    }
}
