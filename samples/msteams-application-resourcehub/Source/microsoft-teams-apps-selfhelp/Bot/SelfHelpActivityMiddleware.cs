namespace Microsoft.Teams.Selfhelp.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Selfhelp.Authentication.Bot;

    /// <summary>
    /// A class that represents middleware that can operate on incoming activities.
    /// </summary>
    public class SelfHelpActivityMiddleware : IMiddleware
    {
        private static readonly string MsTeamsChannelId = "msteams";
        private readonly bool disableTenantFilter;
        private readonly string allowedTenants;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfHelpActivityMiddleware"/> class.
        /// </summary>
        /// <param name="botFilterMiddlewareOptions">The bot filter middleware options.</param>
        public SelfHelpActivityMiddleware(IOptions<BotFilterMiddlewareOptions> botFilterMiddlewareOptions)
        {
            this.disableTenantFilter = botFilterMiddlewareOptions.Value.DisableTenantFilter;
            this.allowedTenants = botFilterMiddlewareOptions.Value.AllowedTenants;
        }

        /// <summary>
        /// Processes an incoming activity.
        /// If the activity's channel id is not "msteams", or its conversation's tenant is not an allowed tenant,
        /// then the middleware short circuits the pipeline, and skips the middlewares and handlers
        /// that are listed after this filter in the pipeline.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of a conversation.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            var isMsTeamsChannel = this.ValidateBotFrameworkChannelId(turnContext);
            if (!isMsTeamsChannel)
            {
                return;
            }

            var isAllowedTenant = this.ValidateTenant(turnContext);
            if (!isAllowedTenant)
            {
                return;
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Validate the bot framework channel id details.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of a conversation.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        private bool ValidateBotFrameworkChannelId(ITurnContext turnContext)
        {
            return SelfHelpActivityMiddleware.MsTeamsChannelId.Equals(
                turnContext?.Activity?.ChannelId,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validate the tenant details.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of a conversation.</param>
        /// <returns>Returns true if details inserted or updated successfully,Else returns false.</returns>
        private bool ValidateTenant(ITurnContext turnContext)
        {
            if (this.disableTenantFilter)
            {
                return true;
            }

            var allowedTenantIds = this.allowedTenants
                ?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                ?.Select(p => p.Trim());
            if (allowedTenantIds == null || !allowedTenantIds.Any())
            {
                var exceptionMessage = "AllowedTenants setting is not set properly in the configuration file.";
                Console.WriteLine(exceptionMessage);
                throw new ApplicationException(exceptionMessage);
            }

            var tenantId = turnContext?.Activity?.Conversation?.TenantId;
            return allowedTenantIds.Contains(tenantId);
        }
    }
}