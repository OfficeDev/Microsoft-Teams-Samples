using MeetingEventsCallingBot.Authentication;
using MeetingEventsCallingBot.Extenstions;
using MeetingEventsCallingBot.Model;
using MeetingEventsCallingBot.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Client.Transport;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Common.Transport;
using Microsoft.Graph.Communications.Core.Notifications;
using Microsoft.Graph.Communications.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MeetingEventsCallingBot.Bots
{
    public class CallingBotService
    {
        private readonly BotOptions options;

        public IGraphLogger GraphLogger { get; }

        private IRequestAuthenticationProvider AuthenticationProvider { get; }

        private INotificationProcessor NotificationProcessor { get; }

        private CommsSerializer Serializer { get; }

        private GraphServiceClient RequestBuilder { get; }

        private IGraphClient GraphApiClient { get; }

        public CallingBotService(BotOptions options, IGraphLogger graphLogger)
        {
            this.options = options;
            this.GraphLogger = graphLogger;
            var name = this.GetType().Assembly.GetName().Name;
            this.AuthenticationProvider = new AuthenticationProvider(name, options.AppId, options.AppSecret, graphLogger);

            this.Serializer = new CommsSerializer();
            var authenticationWrapper = new AuthenticationWrapper(this.AuthenticationProvider);
            this.NotificationProcessor = new NotificationProcessor(Serializer);
            this.NotificationProcessor.OnNotificationReceived += this.NotificationProcessor_OnNotificationReceived;

            this.RequestBuilder = new GraphServiceClient(options.PlaceCallEndpointUrl.AbsoluteUri, authenticationWrapper);

            var defaultProperties = new List<IGraphProperty<IEnumerable<string>>>();
            using (HttpClient tempClient = GraphClientFactory.Create(authenticationWrapper))
            {
                defaultProperties.AddRange(tempClient.DefaultRequestHeaders.Select(header => GraphProperty.RequestProperty(header.Key, header.Value)));
            }

            // graph client
            var productInfo = new ProductInfoHeaderValue(
                typeof(CallingBotService).Assembly.GetName().Name,
                typeof(CallingBotService).Assembly.GetName().Version.ToString());
            this.GraphApiClient = new GraphAuthClient(
                this.GraphLogger,
                this.Serializer.JsonSerializerSettings,
                new HttpClient(),
                this.AuthenticationProvider,
                productInfo,
                defaultProperties);
        }

        /// <summary>
        /// Process "/api/calss" notifications asyncronously. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task ProcessNotificationAsync(
            HttpRequest request,
            HttpResponse response)
        {
            try
            {
                var httpRequest = request.CreateRequestMessage();
                var results = await this.AuthenticationProvider.ValidateInboundRequestAsync(httpRequest).ConfigureAwait(false);
                if (results.IsValid)
                {
                    var httpResponse = await this.NotificationProcessor.ProcessNotificationAsync(httpRequest).ConfigureAwait(false);
                    await httpResponse.CreateHttpResponseAsync(response).ConfigureAwait(false);
                }
                else
                {
                    var httpResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    await httpResponse.CreateHttpResponseAsync(response).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync(e.ToString()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called when INotificationProcessor recieves notification.
        /// </summary>
        /// <param name="args"></param>
        private void NotificationProcessor_OnNotificationReceived(NotificationEventArgs args)
        {
            _ = NotificationProcessor_OnNotificationReceivedAsync(args).ForgetAndLogExceptionAsync(
                this.GraphLogger,
                $"Error processing notification {args.Notification.ResourceUrl} with scenario {args.ScenarioId}");
        }

        private async Task NotificationProcessor_OnNotificationReceivedAsync(NotificationEventArgs args)
        {
            this.GraphLogger.CorrelationId = args.ScenarioId;

            if (args.Notification.ResourceUrl.EndsWith("/participants") && args.ResourceData is List<object> participantObjects)
            {
                this.GraphLogger.Log(TraceLevel.Info, "Total count of participants found in this roster is " + participantObjects.Count());

                string[] urlSplit = args.Notification.ResourceUrl.Split("/");
                var callId = urlSplit[urlSplit.Length - 2];

                // Use this list of participants to get real-time participants in the call
                List<ParticipantDetails> participantDetails = await GetListOfParticipantsInCall(participantObjects, args.TenantId, callId, args.ScenarioId);
            }
        }

        /// <summary>
        /// Get list of all participants present in the call.
        /// </summary>
        /// <param name="participantObjects"></param>
        /// <param name="argsTenantId"></param>
        /// <param name="callId"></param>
        /// <param name="scenarioId"></param>
        /// <returns>List of ParticipantDetails</returns>
        private async Task<List<ParticipantDetails>> GetListOfParticipantsInCall(List<object> participantObjects, string argsTenantId, string callId, Guid scenarioId)
        {
            var participantDetailsList = new HashSet<ParticipantDetails>();
            string botParticipantId = "";
            foreach (var participantObject in participantObjects)
            {
                var participant = participantObject as Participant;
                var participantDetailsObject = new ParticipantDetails();

                // Identity User object for bot is null
                if (participant?.Info?.Identity?.User != null)
                {
                    string aadUserId = participant.Info.Identity.User.Id;
                    string tenantId = (string)participant.Info.Identity.User.AdditionalData["tenantId"];
                    string participantName = participant.Info.Identity.User.DisplayName.ToString();

                    participantDetailsObject.Name = participantName;
                    participantDetailsObject.UserId = aadUserId;
                    participantDetailsObject.TenantId = tenantId;
                }
                else if (participant?.Info?.Identity?.AdditionalData != null)
                {
                    if (participant.Info.Identity.AdditionalData.ContainsKey("guest") && participant.IsInLobby == false)
                    {
                        Identity guestDisplayName = (Identity)participant.Info.Identity.AdditionalData["guest"];
                        participantDetailsObject.Name = guestDisplayName.DisplayName + " (Guest)\n";
                    }
                    else if (participant.Info.Identity.AdditionalData.ContainsKey("phone") && participant.IsInLobby == false)
                    {
                        Identity phoneDisplayName = (Identity)participant.Info.Identity.AdditionalData["phone"];
                        participantDetailsObject.Name = phoneDisplayName.DisplayName + " (Phone)\n";
                    } 
                    else
                    {
                        botParticipantId = participant.Id;
                    }
                }
                if (participantDetailsObject.Name != null)
                {
                    participantDetailsList.Add(participantDetailsObject);
                }
            }

            if (participantDetailsList.Count == 0 && botParticipantId != "")
            {
                var hangupRequest = this.RequestBuilder.Communications.Calls[callId].Request();
                await this.GraphApiClient.SendAsync(hangupRequest, RequestType.Delete, argsTenantId, scenarioId).ConfigureAwait(false);
            }
            
            return participantDetailsList.ToList();
        }
    }
}
