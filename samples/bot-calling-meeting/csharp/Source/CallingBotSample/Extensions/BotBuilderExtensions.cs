// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CallingBotSample.Bots;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CallingBotSample.Extensions
{
    public static class BotBuilderExtensions
    {
        public static IServiceCollection AddBot(this IServiceCollection services)
            => services.AddBot(_ => { });

        public static IServiceCollection AddBot(this IServiceCollection services, Action<BotOptions> botOptionsAction)
        {
            var options = new BotOptions();
            botOptionsAction(options);
            services.AddSingleton(options);

            return services.AddTransient<CallingBot>();
        }
    }
}
