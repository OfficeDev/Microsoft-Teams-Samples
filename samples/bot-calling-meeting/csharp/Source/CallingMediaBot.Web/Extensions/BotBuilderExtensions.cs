namespace CallingMediaBot.Web.Extensions;

using CallingMediaBot.Web.Bots;
using CallingMediaBot.Web.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

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
