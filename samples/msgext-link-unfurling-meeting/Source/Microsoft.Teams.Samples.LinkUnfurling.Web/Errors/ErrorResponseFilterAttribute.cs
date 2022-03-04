// <copyright file="ErrorResponseFilterAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Errors
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Identity.Web;

    /// <summary>
    /// Handles filtered exceptions and prepares error response.
    /// </summary>
    public sealed class ErrorResponseFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseFilterAttribute"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ErrorResponseFilterAttribute(ILogger<ErrorResponseFilterAttribute> logger)
        {
            this.Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets Logger.
        /// </summary>
        public ILogger<ErrorResponseFilterAttribute> Logger { get; }

        /// <inheritdoc/>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // No error.
            if (context.Exception == null)
            {
                return;
            }

            if (context.Exception is ServiceException exception)
            {
                if (context.Exception?.InnerException is MicrosoftIdentityWebChallengeUserException || exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.Logger.LogWarning(exception, $"User hasn't consented. Http Status Code: {exception.StatusCode}, ErrorCode: {exception.Error.Code}");
                    context.Result = this.GetObjectResult(HttpStatusCode.Forbidden, exception.Error.Code, exception);
                    context.ExceptionHandled = true;
                }
            }
            else
            {
                this.Logger.LogError(context.Exception, "Unknown exception");
            }
        }

        /// <summary>
        /// Prepares ObjectResult.
        /// </summary>
        /// <param name="exception">QBotException instance.</param>
        /// <returns>Object result.</returns>
        private ObjectResult GetObjectResult(HttpStatusCode statusCode, string errorCode, Exception exception)
        {
            var code = errorCode;
            var error = new ApiError()
            {
                Error = new CustomError()
                {
                    Code = code,
                    Message = exception.Message,
                    InnerError = new InnerError()
                    {
                        Message = exception.InnerException?.Message,
                    },
                },
            };

            return new ObjectResult(error)
            {
                StatusCode = (int)statusCode,
            };
        }
    }
}