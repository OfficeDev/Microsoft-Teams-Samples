// Teamsbot.cs for Proactive Bot
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots // Replace with your actual namespace
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Authentication and Adapter setup
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Register ConversationReference dictionary for proactive messaging
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Register the bot
            services.AddTransient<IBot, TeamsBot>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseRouting()
               .UseAuthorization()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });
        }
    }

    public class TeamsBot : TeamsActivityHandler
    {
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IConfiguration _configuration;

        public TeamsBot(
            ConcurrentDictionary<string, ConversationReference> conversationReferences,
            IConfiguration configuration)
        {
            _conversationReferences = conversationReferences;
            _configuration = configuration;
        }

        private void AddConversationReference(Activity activity)
        {
            var reference = activity.GetConversationReference();
            _conversationReferences[reference.Conversation.Id] = reference;
        }

        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var endpoint = _configuration["BOT_ENDPOINT"];
                    var welcomeMessage = $"Welcome to the Proactive Bot sample. Navigate to {endpoint}/api/notify to proactively message everyone who has previously messaged this bot.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeMessage), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            var endpoint = _configuration["BOT_ENDPOINT"];
            var url = $"{endpoint}/api/notify";

            var message = $"You sent '{turnContext.Activity.Text}'. Navigate to [{url}]({url}) to proactively message everyone who has previously messaged this bot.";

            var reply = MessageFactory.Text(message);
            reply.TextFormat = "markdown";  // Enables clickable markdown links

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
