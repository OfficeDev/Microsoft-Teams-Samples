// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Tests.Middleware
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Middleware;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class JsonExceptionHandlerTests
    {
        [TestMethod]
        public async Task JsonExceptionHandler_ArgumentNullException_Returns500NoMessageAndErrorCode()
        {
            // Arrange
            string exceptionMessage = "This message should not be returned to the user.";

            var httpContext = new DefaultHttpContext();
            var exceptionHandlingMiddleware =
                CreateExceptionHandler(httpContext, CreateMiddleware(new ArgumentNullException(exceptionMessage)));

            // Act
            await exceptionHandlingMiddleware.InvokeAsync(httpContext);
            var bodyContent = ReadHttpContextBody(httpContext);

            // Assert
            Assert.AreEqual(500, httpContext.Response.StatusCode);
            Assert.IsTrue(bodyContent.Contains("\"Message\":null", StringComparison.InvariantCulture));
            Assert.IsTrue(bodyContent.Contains($"\"ErrorCode\":\"Unknown\"", StringComparison.InvariantCulture));
        }

        [TestMethod]
        public async Task JsonExceptionHandler_ApiArgumentNullException_Returns400AndMessageAndErrorCodeMatches()
        {
            // Arrange
            string exceptionMessage = "This message should be returned to the user.";

            var httpContext = new DefaultHttpContext();
            var exceptionHandlingMiddleware =
                CreateExceptionHandler(httpContext, CreateMiddleware(new ApiArgumentNullException(exceptionMessage)));

            // Act
            await exceptionHandlingMiddleware.InvokeAsync(httpContext);
            var bodyContent = ReadHttpContextBody(httpContext);

            // Assert
            Assert.AreEqual(400, httpContext.Response.StatusCode);
            Assert.IsTrue(bodyContent.Contains($"\"Message\":\"{exceptionMessage}\"", StringComparison.InvariantCulture));
            Assert.IsTrue(bodyContent.Contains($"\"ErrorCode\":\"ArgumentNotValid\"", StringComparison.InvariantCulture));
        }

        [TestMethod]
        public async Task JsonExceptionHandler_ApiArgumentException_Returns400AndMessageAndErrorCodeMatches()
        {
            // Arrange
            string exceptionMessage = "This message should be returned to the user.";

            var httpContext = new DefaultHttpContext();
            var exceptionHandlingMiddleware =
                CreateExceptionHandler(httpContext, CreateMiddleware(new ApiArgumentException(exceptionMessage)));

            // Act
            await exceptionHandlingMiddleware.InvokeAsync(httpContext);
            var bodyContent = ReadHttpContextBody(httpContext);

            // Assert
            Assert.AreEqual(400, httpContext.Response.StatusCode);
            Assert.IsTrue(bodyContent.Contains($"\"Message\":\"{exceptionMessage}\"", StringComparison.InvariantCulture));
            Assert.IsTrue(bodyContent.Contains($"\"ErrorCode\":\"ArgumentNotValid\"", StringComparison.InvariantCulture));
        }

        [TestMethod]
        public async Task JsonExceptionHandler_ApiException_ReturnsRelevantStatusCodeAndMessageMatches()
        {
            // Arrange
            var statusCode = HttpStatusCode.Forbidden;
            var errorCode = ErrorCode.Forbidden;
            string exceptionMessage = "This message should be returned to the user.";

            var httpContext = new DefaultHttpContext();
            var exceptionHandlingMiddleware =
                CreateExceptionHandler(httpContext,
                    CreateMiddleware(new ApiException(statusCode, errorCode, exceptionMessage)));

            // Act
            await exceptionHandlingMiddleware.InvokeAsync(httpContext);
            var bodyContent = ReadHttpContextBody(httpContext);

            // Assert
            Assert.AreEqual((int) statusCode, httpContext.Response.StatusCode);
            Assert.IsTrue(bodyContent.Contains($"\"Message\":\"{exceptionMessage}\"", StringComparison.InvariantCulture));
            Assert.IsTrue(bodyContent.Contains($"\"ErrorCode\":\"{errorCode}\"", StringComparison.InvariantCulture));
        }

        private RequestDelegate CreateMiddleware(Exception exception)
        {
            return (HttpContext) =>
            {
                return Task.FromException(exception);
            };
        }

        private JsonExceptionHandler CreateExceptionHandler(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            httpContext.Response.Body = new MemoryStream();
            return new JsonExceptionHandler(requestDelegate);
        }

        private string ReadHttpContextBody(HttpContext httpContext)
        {
            httpContext.Response.Body.Position = 0;
            var bodyContent = "";

            using (var sr = new StreamReader(httpContext.Response.Body))
            {
                bodyContent = sr.ReadToEnd();
            }

            return bodyContent;
        }
    }
}
