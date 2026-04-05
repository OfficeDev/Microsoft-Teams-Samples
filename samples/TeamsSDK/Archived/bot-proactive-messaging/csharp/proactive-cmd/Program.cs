using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Microsoft.Teams.Samples.ProactiveMessageCmd
{
    class Program
    {
        static readonly Random random = new Random();

        // Create the send policy for Microsoft Teams
        // For more information about these policies
        // see: http://www.thepollyproject.org/
        static ResiliencePipeline CreatePolicy()
        {
            var transientRetry = new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ErrorResponseException>(ex => ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase)),
                MaxRetryAttempts = 3,
                DelayGenerator = args => new ValueTask<TimeSpan?>(
                    TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber + 1)) +
                    TimeSpan.FromMilliseconds(random.Next(0, 1000))),
            };

            var circuitBreaker = new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ErrorResponseException>(ex => ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase)),
                FailureRatio = 1.0,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromMinutes(10),
                BreakDuration = TimeSpan.FromMinutes(10),
            };

            var longTermRetry = new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<BrokenCircuitException>(),
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMinutes(10),
            };

            return new ResiliencePipelineBuilder()
                .AddRetry(longTermRetry)
                .AddCircuitBreaker(circuitBreaker)
                .AddRetry(transientRetry)
                .Build();
        }

        static readonly ResiliencePipeline RetryPolicy = CreatePolicy();

        static Task SendWithRetries(Func<Task> callback)
        {
            return RetryPolicy.ExecuteAsync(async _ =>
            {
                await callback();
            }).AsTask();
        }

        static Task<int> Main(string[] args)
        {
            var appIdOption = new Option<string>("--app-id") 
            {
                Required = true
            };

            var appPasswordOption = new Option<string>("--app-password") 
            {
                Required = true
            };

            var tenantIdOption = new Option<string>("--tenant-id") 
            {
                Required = true
            };

            var messageOption = new Option<string>("--message")
            {
                Required = true
            };
            messageOption.Aliases.Add("-m");

            var serviceUrlOption = new Option<string>("--service-url")
            {
                Required = true
            };
            serviceUrlOption.Aliases.Add("-s");

            var conversationIdOption = new Option<string>("--conversation-id")
            {
                Required = true
            };
            conversationIdOption.Aliases.Add("-c");

            var channelIdOption = new Option<string>("--channel-id")
            {
                Required = true
            };
            channelIdOption.Aliases.Add("-c");

            var sendUserMessageCommand = new Command("sendUserMessage", "Send a message to the conversation coordinates")
            {
                appIdOption,
                appPasswordOption,
                tenantIdOption,
                serviceUrlOption,
                conversationIdOption,
                messageOption
            };
            sendUserMessageCommand.SetAction(async parseResult =>
            {
                var appId = parseResult.GetValue(appIdOption);
                var appPassword = parseResult.GetValue(appPasswordOption);
                var tenantId = parseResult.GetValue(tenantIdOption);
                var serviceUrl = parseResult.GetValue(serviceUrlOption);
                var conversationId = parseResult.GetValue(conversationIdOption);
                var message = parseResult.GetValue(messageOption);

                if (!AreRequiredValuesPresent(appId, appPassword, tenantId, serviceUrl, conversationId, message))
                {
                    return 1;
                }

                await SendToUserAsync(appId, appPassword, tenantId, serviceUrl, conversationId, message);
                return 0;
            });

            var createChannelThreadCommand = new Command("createThread", "Create a new thread in a channel")
            {
                appIdOption,
                appPasswordOption,
                tenantIdOption,
                serviceUrlOption,
                channelIdOption,
                messageOption
            };
            createChannelThreadCommand.SetAction(async parseResult =>
            {
                var appId = parseResult.GetValue(appIdOption);
                var appPassword = parseResult.GetValue(appPasswordOption);
                var tenantId = parseResult.GetValue(tenantIdOption);
                var serviceUrl = parseResult.GetValue(serviceUrlOption);
                var channelId = parseResult.GetValue(channelIdOption);
                var message = parseResult.GetValue(messageOption);

                if (!AreRequiredValuesPresent(appId, appPassword, tenantId, serviceUrl, channelId, message))
                {
                    return 1;
                }

                await CreateChannelThreadAsync(appId, appPassword, tenantId, serviceUrl, channelId, message);
                return 0;
            });

            var sendChannelThreadMessageCommand = new Command("sendChannelThread", "Send a message to a channel thread")
            {
                appIdOption,
                appPasswordOption,
                tenantIdOption,
                serviceUrlOption,
                conversationIdOption,
                messageOption
            };
            sendChannelThreadMessageCommand.SetAction(async parseResult =>
            {
                var appId = parseResult.GetValue(appIdOption);
                var appPassword = parseResult.GetValue(appPasswordOption);
                var tenantId = parseResult.GetValue(tenantIdOption);
                var serviceUrl = parseResult.GetValue(serviceUrlOption);
                var conversationId = parseResult.GetValue(conversationIdOption);
                var message = parseResult.GetValue(messageOption);

                if (!AreRequiredValuesPresent(appId, appPassword, tenantId, serviceUrl, conversationId, message))
                {
                    return 1;
                }

                await SendToThreadAsync(appId, appPassword, tenantId, serviceUrl, conversationId, message);
                return 0;
            });

            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                sendUserMessageCommand,
                createChannelThreadCommand,
                sendChannelThreadMessageCommand
            };

            // Parse the incoming args and invoke the handler
            return rootCommand.Parse(args).InvokeAsync();
        }

        static bool AreRequiredValuesPresent(params string[] values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.Error.WriteLine("All required options must be non-empty values.");
                    return false;
                }
            }

            return true;
        }

        /// Send a one-on-one message to a user.
        /// This method also makes the message appear in the activity feed!
        public static async Task SendToUserAsync(string appId, string appPassword, string tenantId, string serviceUrl, string conversationId, string message)
        {
            var activity = MessageFactory.Text(message);
            activity.Summary = message; // Ensure that the summary text is populated so the toast notifications aren't generic text.
            activity.TeamsNotifyUser(); // Send the message into the activity feed.

            var credentials = new MicrosoftAppCredentials(appId, appPassword) 
            {
                ChannelAuthTenant = tenantId
            };

            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            await SendWithRetries(async () => 
                    await connectorClient.Conversations.SendToConversationAsync(conversationId, activity));
        }

        /// Create a new thread in a channel.
        public static async Task CreateChannelThreadAsync(string appId, string appPassword, string tenantId, string serviceUrl, string channelId, string message)
        {
            // Create the connector client using the service url & the bot credentials.
            var credentials = new MicrosoftAppCredentials(appId, appPassword) 
            {
                ChannelAuthTenant = tenantId
            };
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            
            // Craft an activity from the message
            var activity = MessageFactory.Text(message);

            var conversationParameters = new ConversationParameters
            {
                  IsGroup = true,
                  ChannelData = new TeamsChannelData
                  {
                      Channel = new ChannelInfo(channelId),
                  },
                  Activity = activity
            };

            await SendWithRetries(async () => 
                    await connectorClient.Conversations.CreateConversationAsync(conversationParameters));
        }

        /// Send a message to a thread in a channel.
        public static async Task SendToThreadAsync(string appId, string appPassword, string tenantId, string serviceUrl, string conversationId, string message)
        {
            var activity = MessageFactory.Text(message);
            activity.Summary = message; // Ensure that the summary text is populated so the toast notifications aren't generic text.

            // For SingleTenant, set the tenant-specific OAuth endpoint
            var credentials = new MicrosoftAppCredentials(appId, appPassword) 
            {
                ChannelAuthTenant = tenantId
            };

            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            await SendWithRetries(async () => 
                    await connectorClient.Conversations.SendToConversationAsync(conversationId, activity));
        }

    }
}
