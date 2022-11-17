// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CallingBotSample.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CallingBotSample.Services.MicrosoftGraph
{
    public class CallService : ICallService
    {
        private readonly GraphServiceClient graphServiceClient;
        private readonly AzureAdOptions azureAdOptions;
        private readonly BotOptions botOptions;

        private readonly string callbackUri;

        public CallService(GraphServiceClient graphServiceClient, IOptions<AzureAdOptions> azureAdOptions, IOptions<BotOptions> botOptions)
        {
            this.graphServiceClient = graphServiceClient;
            this.azureAdOptions = azureAdOptions.Value;
            this.botOptions = botOptions.Value;

            callbackUri = new Uri(botOptions.Value.BotBaseUrl, "callback").ToString();
        }

        /// <inheritdoc/>
        public Task Answer(string id, params MediaInfo[]? preFetchMedia)
        {
            return graphServiceClient.Communications.Calls[id]
                .Answer(
                    callbackUri: callbackUri,
                    mediaConfig: new ServiceHostedMediaConfig
                    {
                        PreFetchMedia = preFetchMedia
                    },
                    acceptedModalities: new List<Modality> { Modality.Audio })
                .Request()
                .PostAsync();
        }

        /// <inheritdoc/>
        public Task<Call> Create(string? threadId = null, Identity? meetingOrganiser = null, params Identity[] users)
        {
            var call = new Call
            {
                Direction = CallDirection.Outgoing,
                CallbackUri = callbackUri,
                ChatInfo = new ChatInfo
                {
                    ThreadId = threadId,
                    MessageId = "0"
                },
                TenantId = azureAdOptions.TenantId,
                // If the meetingOrganiser is provided we don't need to invite people directly.
                Targets = meetingOrganiser != null ? null : users.Select(user => new InvitationParticipantInfo
                {
                    Identity = new IdentitySet
                    {
                        User = user
                    }
                }),
                MeetingInfo = meetingOrganiser == null ? null : new OrganizerMeetingInfo()
                {
                    Organizer = new IdentitySet
                    {
                        User = meetingOrganiser
                    }
                },

                RequestedModalities = new List<Modality>()
            {
                Modality.Audio
            },
                MediaConfig = new ServiceHostedMediaConfig
                {
                }
            };

            return graphServiceClient.Communications.Calls
                .Request()
                .AddAsync(call);
        }

        /// <inheritdoc/>
        public Task<Call> Get(string id)
        {
            return graphServiceClient.Communications.Calls[id]
                .Request()
                .GetAsync();
        }

        /// <inheritdoc/>
        public Task HangUp(string id)
        {
            return graphServiceClient.Communications.Calls[id]
                .Request()
                .DeleteAsync();
        }

        /// <inheritdoc/>
        public Task InviteParticipant(string id, params IdentitySet[] participants)
        {
            var invitationParticipants = participants.Select(participant =>
                new InvitationParticipantInfo
                {
                    Identity = participant
                });

            return graphServiceClient.Communications.Calls[id].Participants
                .Invite(invitationParticipants, id)
                .Request()
                .PostAsync();
        }

        /// <inheritdoc />
        public Task<PlayPromptOperation> PlayPrompt(string id, params MediaInfo[] mediaPrompts)
        {
            return graphServiceClient.Communications.Calls[id]
                .PlayPrompt(
                    CreatePromptsFromMediaInfos(mediaPrompts),
                    clientContext: id)
                .Request()
                .PostAsync();
        }

        /// <inheritdoc/>
        public Task<RecordOperation> Record(
            string id,
            MediaInfo mediaPrompt,
            int maxRecordDurationInSeconds = 10,
            IEnumerable<string>? stopTones = null)
        {
            if (stopTones == null)
            {
                stopTones = new List<string>()
            {
                "#",
                "1",
                "*"
            };
            }

            return graphServiceClient.Communications.Calls[id]
                .RecordResponse(
                    CreatePromptsFromMediaInfos(new List<MediaInfo>() { mediaPrompt }),
                    bargeInAllowed: null,
                    initialSilenceTimeoutInSeconds: null,
                    maxSilenceTimeoutInSeconds: null,
                    maxRecordDurationInSeconds,
                    playBeep: null,
                    stopTones,
                    clientContext: id)
                .Request()
                .PostAsync();

        }

        /// <inheritdoc/>
        public Task<Call> Redirect(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Reject(string id, RejectReason rejectReason)
        {
            return graphServiceClient.Communications.Calls[id]
                .Reject(rejectReason, null)
                .Request()
                .PostAsync();
        }

        /// <inheritdoc/>
        public Task<Call> Reject(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Transfer(string id, Identity transferIdentity, Identity? transfereeIdentity = null)
        {
            var transferTarget = new InvitationParticipantInfo
            {
                Identity = new IdentitySet
                {
                    User = transferIdentity
                },
                AdditionalData = new Dictionary<string, object>()
            {
                {"endpointType", "default"}
            }
            };

            ParticipantInfo? transferee = null;
            if (transfereeIdentity != null)
            {
                if (transfereeIdentity.AdditionalData == null)
                {
                    transfereeIdentity.AdditionalData = new Dictionary<string, object>();
                }
                transfereeIdentity.AdditionalData["tenantId"] = azureAdOptions.TenantId;

                transferee = new ParticipantInfo
                {
                    Identity = new IdentitySet
                    {
                        User = transfereeIdentity
                    },
                };
            }

            return graphServiceClient.Communications.Calls[id]
                .Transfer(transferTarget, transferee)
                .Request()
                .PostAsync();
        }

        private IEnumerable<Prompt> CreatePromptsFromMediaInfos(IEnumerable<MediaInfo> mediaInfos)
        {
            return mediaInfos.Select(mediaPrompt =>
                new MediaPrompt
                {
                    MediaInfo = mediaPrompt
                });
        }
    }
}
