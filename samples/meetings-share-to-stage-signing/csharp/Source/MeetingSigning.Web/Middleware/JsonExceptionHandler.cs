// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Middleware
{
    using System;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Models;

    public class JsonExceptionHandler
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nextPipe">Next request handler in the pipeline to be processed.</param>
        public JsonExceptionHandler(RequestDelegate nextPipe)
        {
            m_nextPipe = nextPipe;
        }

        /// <summary>
        /// Executes the rest of the pipeline but catches all the exceptions to return the proper response.
        /// </summary>
        /// <param name="httpContext">The http context.</param>
        /// <returns>Task</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await m_nextPipe(httpContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handle Exception
        /// </summary>
        /// <param name="httpContext">Http Context</param>
        /// <param name="exception">Exception</param>
        /// <returns>Task</returns>
        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            ApiErrorResponse errorResponse;
            switch (exception)
            {
                case ApiException:
                    ApiException apiException = (ApiException) exception;

                    httpContext.Response.StatusCode = (int)(apiException.StatusCode);
                    errorResponse = new ApiErrorResponse { Message = exception.Message, ErrorCode = apiException.Code };
                    break;
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    // Do not return an internal Error Message unless it is clear that it might be shown to the user.
                    errorResponse = new ApiErrorResponse { ErrorCode = ErrorCode.Unknown };
                    break;
            }

            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            JsonSerializerOptions options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }};
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options)).ConfigureAwait(false);
        }

        private readonly RequestDelegate m_nextPipe;
    }
}
