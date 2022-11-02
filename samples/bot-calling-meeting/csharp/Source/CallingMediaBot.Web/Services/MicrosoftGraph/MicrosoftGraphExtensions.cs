// Copyright (c) Microsoft Corporation. All rights reserved.

namespace CallingMediaBot.Web.Services.MicrosoftGraph;

using CallingMediaBot.Domain.Interfaces;
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
