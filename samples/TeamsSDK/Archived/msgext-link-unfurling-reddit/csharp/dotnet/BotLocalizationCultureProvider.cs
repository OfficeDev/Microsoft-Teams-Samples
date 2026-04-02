// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit;

using System.Text.Json;
using Microsoft.AspNetCore.Localization;

/// <summary>
/// The BotLocalizationCultureProvider is responsible for implementing the <see cref="IRequestCultureProvider"/> for Bot Activities
/// received from BotFramework.
/// </summary>
internal sealed class BotLocalizationCultureProvider : IRequestCultureProvider
{
    /// <summary>
    /// Get the culture of the current request.
    /// </summary>
    /// <param name="httpContext">The current request.</param>
    /// <returns>A Task resolving to the culture info if found, null otherwise.</returns>
    public async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext?.Request?.Body?.CanRead != true)
        {
            return null;
        }

        var isBotFrameworkUserAgent = httpContext.Request.Headers["User-Agent"]
            .Any(userAgent => userAgent != null && userAgent.Contains("Microsoft-BotFramework", StringComparison.OrdinalIgnoreCase));

        if (!isBotFrameworkUserAgent)
        {
            return null;
        }

        try
        {
            httpContext.Request.EnableBuffering();

            using var jsonDoc = await JsonDocument.ParseAsync(httpContext.Request.Body);
            if (jsonDoc.RootElement.TryGetProperty("locale", out var localeElement))
            {
                var locale = localeElement.GetString();
                if (!string.IsNullOrEmpty(locale))
                {
                    return new ProviderCultureResult(locale);
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        }
    }
}