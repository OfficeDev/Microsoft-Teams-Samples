// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace CallingBotSample.Cache
{
    public static class CacheExtensions
    {
        /// <summary>
        /// Adds Caches
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCaches(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICallCache, CallCache>();
            services.AddSingleton<IIncidentCache, IncidentCache>();

            return services;
        }
    }
}
