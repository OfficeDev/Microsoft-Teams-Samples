namespace Microsoft.Teams.Apps.QBot.Web.Middlewares
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Request context extensions.
    /// </summary>
    public static class RequestContextMiddlewareExtensions
    {
        /// <summary>
        /// Adds request context middleware.
        /// </summary>
        /// <param name="builder">App builder.</param>
        /// <returns>App builder instance.</returns>
        public static IApplicationBuilder UseRequestContext(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestContextMiddleware>();
        }
    }
}
