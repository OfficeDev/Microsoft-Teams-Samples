// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.GraphService
{
    using System;
    using System.Net;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Identity.Web;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;

    /// <summary>
    /// Base class to handle scenarios across multiple Microsoft Graph calls.
    /// </summary>
    public class GraphServiceHelper
    {
        internal readonly ILogger<GraphServiceHelper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftGraphService"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GraphServiceHelper(
            ILogger<GraphServiceHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles an exception thrown by graph, and returns an exception that can allow for things to be handled on the frontend
        /// </summary>
        /// <param name="exception">The exception thrown during a Graph call</param>
        /// <returns></returns>
        public ApiException HandleGraphExceptions(Exception exception)
        {
            if (exception is MicrosoftIdentityWebChallengeUserException ||
                exception.InnerException is MicrosoftIdentityWebChallengeUserException ||
                exception.Message.Contains("Authorization_RequestDenied", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogError(exception, "Consent required");
                // This exception indicates consent is required.
                // Return a 403 with "consent_required" in the body
                // to signal to the tab it needs to prompt for consent
                return new ApiException(HttpStatusCode.Forbidden, ErrorCode.AuthConsentRequired, "Consent Required");
            }
            else if (exception is ServiceException)
            {
                ServiceException serviceException = (ServiceException)exception;
                _logger.LogError($"Graph Service Exception: Message: '{serviceException.Message}'; ResponseBody: '{serviceException.RawResponseBody}'", serviceException);
                return new ApiException(serviceException.StatusCode, ErrorCode.GraphServiceException, "Graph Service Exception");
            }
            else
            {
                _logger.LogError(exception, "Error occurred");
                return new ApiException(HttpStatusCode.InternalServerError, ErrorCode.Unknown);
            }
        }
    }
}
