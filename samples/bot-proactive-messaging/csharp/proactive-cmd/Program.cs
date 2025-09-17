using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Polly;
using Polly.CircuitBreaker;

namespace Microsoft.Teams.Samples.ProactiveMessageCmd
{
    class Program
    {
        static readonly Random random = new Random();

        // Create the send policy for Microsoft Teams
        // For more information about these policies
        // see: http://www.thepollyproject.org/
        static IAsyncPolicy CreatePolicy() {
            // Policy for handling the short-term transient throttling.
            // Retry on throttling, up to 3 times with a 2,4,8 second delay between with a 0-1s jitter.
            var transientRetryPolicy = Policy
                    .Handle<ErrorResponseException>(ex => ex.Message.Contains("429"))
                    .WaitAndRetryAsync(
                        retryCount: 3, 
                        (attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(random.Next(0, 1000)));

            // Policy to avoid sending even more messages when the long-term throttling occurs.
            // After 5 messages fail to send, the circuit breaker trips & all subsequent calls will throw
            // a BrokenCircuitException for 10 minutes.
            // Note, in this application this cannot trip since it only sends one message at a time!
            // This is left in for completeness / demonstration purposes.
            var circuitBreakerPolicy = Policy
                .Handle<ErrorResponseException>(ex => ex.Message.Contains("429"))
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 5, TimeSpan.FromMinutes(10));
            
            // Policy to wait and retry when long-term throttling occurs. 
            // This will retry a single message up to 5 times with a 10 minute delay between each attempt.
            // Note, in this application this cannot trip since the circuit breaker above cannot trip.
            // This is left in for completeness / demonstration purposes.
            var outerRetryPolicy = Policy
                .Handle<BrokenCircuitException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    (_) => TimeSpan.FromMinutes(10));
            
            // Combine all three policies so that it will first attempt to retry short-term throttling (inner-most)
            // After 15 (5 messages, 3 failures each) consecutive failed attempts to send a message it will trip the circuit breaker
            // which will fail all messages for the next ten minutes. It will attempt to send messages up to 5 times for a total
            // wait of 50 minutes before failing a message.
            return
                outerRetryPolicy.WrapAsync(
                    circuitBreakerPolicy.WrapAsync(
                        transientRetryPolicy));
        }

        static readonly IAsyncPolicy RetryPolicy = CreatePolicy();

        static Task SendWithRetries(Func<Task> callback)
        {
            return RetryPolicy.ExecuteAsync(callback);
        }

        static Task<int> Main(string[] args)
        {
            static string NonNullOrWhitespace(ArgumentResult symbol)
            {
                return symbol.Tokens
                    .Where(token => string.IsNullOrWhiteSpace(token.Value))
                    .Select(token => $"{symbol.Argument.Name} cannot be null or empty")
                    .FirstOrDefault();
            }

            var appIdOption = new Option<string>("--app-id") 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            appIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var appPasswordOption = new Option<string>("--app-password") 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            appPasswordOption.Argument.AddValidator(NonNullOrWhitespace);

            var tenantIdOption = new Option<string>("--tenant-id") 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            tenantIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var messageOption = new Option<string>(new string[] { "--message", "-m" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            messageOption.Argument.AddValidator(NonNullOrWhitespace);

            var serviceUrlOption = new Option<string>(new string[] { "--service-url", "-s" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            serviceUrlOption.Argument.AddValidator(NonNullOrWhitespace);

            var conversationIdOption = new Option<string>(new string[] { "--conversation-id",  "-c" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            conversationIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var channelIdOption = new Option<string>(new string[] { "--channel-id", "-c" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            channelIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var notifyOption = new Option<bool>("--notify")
            {
                Argument = new Argument<bool> { Arity = ArgumentArity.ExactlyOne }
            };

            var sendUserMessageCommand = new Command("sendUserMessage", "Send a message to the conversation coordinates")
            {
                appIdOption,
                appPasswordOption,
                tenantIdOption,
                serviceUrlOption,
                conversationIdOption,
                messageOption,
                notifyOption
            };
            sendUserMessageCommand.Handler = CommandHandler.Create<string, string, string, string, string, string>(SendToUserAsync);

            var createChannelThreadCommand = new Command("createThread", "Create a new thread in a channel")
            {
                appIdOption,
                appPasswordOption,
                tenantIdOption,
                serviceUrlOption,
                channelIdOption,
                messageOption
            };
            createChannelThreadCommand.Handler = CommandHandler.Create<string, string, string, string, string, string>(CreateChannelThreadAsync);

            var sendChannelThreadMessageCommand = new Command("sendChannelThread", "Send a message to a channel thread")
            {
                appIdOption,
                appPasswordOption,
                tenantIdOption,
                serviceUrlOption,
                conversationIdOption,
                messageOption,
                notifyOption
            };
            sendChannelThreadMessageCommand.Handler = CommandHandler.Create<string, string, string, string, string, string>(SendToThreadAsync);

            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                sendUserMessageCommand,
                createChannelThreadCommand,
                sendChannelThreadMessageCommand
            };


            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args);
        }

        /// Send a one-on-one message to a user.
        /// This method also makes the message appear in the activity feed!
        public static async Task SendToUserAsync(string appId, string appPassword, string tenantId, string serviceUrl, string conversationId, string message)
        {
            var activity = MessageFactory.Text(message);
            activity.Summary = message; // Ensure that the summary text is populated so the toast notifications aren't generic text.
            activity.TeamsNotifyUser(); // Send the message into the activity feed.

            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl); // Required or the activity will be sent w/o auth headers.

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

            // Ensure the service url is marked as "trusted" so the SDK will send auth headers
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            
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

            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl); // Required or the activity will be sent w/o auth headers.

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
