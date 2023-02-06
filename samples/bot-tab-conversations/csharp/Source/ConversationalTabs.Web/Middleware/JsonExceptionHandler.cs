// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Middleware;

using System;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Models;

/// <summary>
/// This middleware catches all exceptions thrown further down pipeline, and returns a JSON formatted error response.
/// By default, a 500 error is thrown, but if the error thrown is of a specific kind (as defined in HandleExceptionAsync),
/// additional information will be shared.
/// </summary>
public class JsonExceptionHandler
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="nextHandler">Next request handler in the pipeline to be processed.</param>
    public JsonExceptionHandler(RequestDelegate nextHandler)
    {
        m_nextHandler = nextHandler;
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
            await m_nextHandler(httpContext).ConfigureAwait(false);
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
                ApiException apiException = (ApiException)exception;

                httpContext.Response.StatusCode = (int)(apiException.StatusCode);
                errorResponse = new ApiErrorResponse(apiException.Code, DateTime.UtcNow, exception.Message);
                break;
            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // Do not return an internal Error Message unless it is clear that it might be shown to the user.
                errorResponse = new ApiErrorResponse(ErrorCode.Unknown, DateTime.UtcNow, "Internal Error");
                break;
        }

        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        JsonSerializerOptions options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options)).ConfigureAwait(false);
    }

    private readonly RequestDelegate m_nextHandler;
}
