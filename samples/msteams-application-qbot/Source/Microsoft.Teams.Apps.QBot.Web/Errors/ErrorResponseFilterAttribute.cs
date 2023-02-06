namespace Microsoft.Teams.Apps.QBot.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;

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

            if (context.Exception is QBotException exception)
            {
                this.Logger.LogWarning(exception, $"QBotException thrown. ErrorCode: {exception.Code}, HttpStatusCode: {exception.StatusCode}.");
                context.Result = this.GetObjectResult(exception);
                context.ExceptionHandled = true;
            }
            else
            {
                this.Logger.LogError(context.Exception, "Unknown exception");
            }
        }

        /// <summary>
        /// Prepares ObjectResult from QBotException.
        /// </summary>
        /// <param name="exception">QBotException instance.</param>
        /// <returns>Object result.</returns>
        private ObjectResult GetObjectResult(QBotException exception)
        {
            var status = exception.StatusCode;
            var code = exception.Code.ToString();
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
                StatusCode = (int)status,
            };
        }
    }
}