using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Web.Http;

namespace Microsoft.Teams.TemplateBotCSharp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Use an in-memory store for bot data.
            // This registers a IBotDataStore singleton that will be used throughout the app.
            var store = new InMemoryDataStore();

            Conversation.UpdateContainer(builder =>
            {
                builder.Register(c => new CachingBotDataStore(store,
                         CachingBotDataStoreConsistencyPolicy
                         .LastWriteWins))
                         .As<IBotDataStore<BotData>>()
                         .AsSelf()
                         .InstancePerLifetimeScope();
            });
        }
    }
}
