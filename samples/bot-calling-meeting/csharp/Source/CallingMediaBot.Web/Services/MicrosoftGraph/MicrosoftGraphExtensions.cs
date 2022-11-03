// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace CallingMediaBot.Web.Services.MicrosoftGraph;
using Microsoft.Extensions.DependencyInjection;

public static class MicrosoftGraphExtensions
{
    /// <summary>
    /// Adds the services that are available in this project to Dependency Injection.
    /// Include this in your Startup.cs ConfigureServices if you need to access these services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collections.</returns>
    public static IServiceCollection AddMicrosoftGraphServices(this IServiceCollection services)
    {
        services.AddScoped<ICallService, CallService>();
        return services;
    }
}
