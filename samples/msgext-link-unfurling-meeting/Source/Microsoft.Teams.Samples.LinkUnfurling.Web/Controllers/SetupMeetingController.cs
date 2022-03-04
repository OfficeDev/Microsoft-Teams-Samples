// <copyright file="SetupMeetingController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.OnlineMeetings;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.Errors;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.ResponseCache;

    /// <summary>
    /// SetupController exposes API to setup a resource review meeting.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ErrorResponseFilterAttribute))]
    public class SetupMeetingController : ControllerBase
    {
        private readonly IMeetingSetup meetingSetup;
        private readonly IResourceProvider resourceProvider;
        private readonly ILogger<SetupMeetingController> logger;
        private readonly IMeetingResponseCache responseCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupMeetingController"/> class.
        /// </summary>
        /// <param name="meetingSetup">Meeting setup.</param>
        /// <param name="resourceProvider">Resource provider.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="responseCache">Resposne cache.</param>
        public SetupMeetingController(
            IMeetingSetup meetingSetup,
            IResourceProvider resourceProvider,
            ILogger<SetupMeetingController> logger,
            IMeetingResponseCache responseCache)
        {
            this.meetingSetup = meetingSetup ?? throw new ArgumentNullException(nameof(meetingSetup));
            this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.responseCache = responseCache ?? throw new ArgumentNullException(nameof(responseCache));
        }

        /// <summary>
        /// Setup online meeting.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>Meeting details.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] SetupRequest request)
        {
            // Validate data
            if (string.IsNullOrEmpty(request.ConversationId) || string.IsNullOrEmpty(request.ResourceId))
            {
                this.logger.LogWarning("Invalid request object. Either chat id of resource id is null or empty.");
                return new BadRequestResult();
            }

            // Check if resource exists. (should also check if user is authorized to access resource. Skipped in the sample.)
            var resource = await this.resourceProvider.GetResourceAsync(request.ResourceId);
            if (resource == null)
            {
                this.logger.LogWarning($"Resource not found! Resource Id from incoming request: {request.ResourceId}");
                return new NotFoundResult();
            }

            // Prepare conversation Context.
            var conversationContext = new ConversationContext()
            {
                ConversationId = request.ConversationId,
                ConversationType = request.ConversationType,
                IsMeetingConversation = !string.IsNullOrEmpty(request.MeetingId),
                MeetingId = request.MeetingId,
                TeamId = request.TeamId,
            };

            var response = await this.SetupReviewMeetingAsync(conversationContext, resource);
            return new OkObjectResult(response);
        }

        private async Task<SetupResponse> SetupReviewMeetingAsync(ConversationContext conversationContext, Resource resource)
        {
            // Prepare meeting object
            // Prepare a unique external id for conversation context and resource.
            // Note: In this sample, we create a unique meeting for a resource in each context where it is shared.
            var externalId = conversationContext.ConversationId + resource.Id;

            // Check if meeting is cached.
            var cachedResponse = this.responseCache.GetMeetingResponseForExternalId(externalId);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            // Else setup a new meeting.
            var meeting = new Meeting()
            {
                Subject = resource.Name,
                StartDateTime = DateTimeOffset.UtcNow.DateTime,
                EndDateTime = DateTimeOffset.UtcNow.AddHours(1).DateTime,
                ExternalId = externalId,
                Participants = new List<ParticipantInfo>()
                {
                    new ParticipantInfo()
                    {
                        AadId = this.User.GetUserId(),
                        Upn = this.User.GetPreferredUserName(),
                        Role = ParticipantRole.Presenter,
                    },
                },
            };

            var newMeeting = await this.meetingSetup.SetupReviewMeetingAsync(meeting, conversationContext, resource, this.User.GetUserId());
            var response = new SetupResponse()
            {
                JoinMeetingLink = newMeeting.Item1,
                TabId = newMeeting.Item2,
            };

            this.responseCache.AddMeetingResponseForExternalId(externalId, response);
            return response;
        }
    }
}
