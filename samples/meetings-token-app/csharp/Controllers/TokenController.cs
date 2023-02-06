// <copyright file="TokenController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using TokenApp.Extensions;
    using TokenApp.Models;
    using TokenApp.Repository;
    using TokenApp.Service;

    /// <summary>
    /// The meeting token controller.
    /// </summary>
    [Route("api")]
    [Authorize]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IMeetingService meetingService;
        private readonly IMeetingTokenRepository meetingTokenRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// </summary>
        /// <param name="meetingService">User Role service.</param>
        /// <param name="meetingTokenRepository">Meeting Token repository.</param>
        public TokenController(
            IMeetingService meetingService,
            IMeetingTokenRepository meetingTokenRepository)
        {
            this.meetingService = meetingService;
            this.meetingTokenRepository = meetingTokenRepository;
        }

        /// <summary>
        /// Get the user role in a meeting.
        /// </summary>
        /// <param name="meetingId">the meeting id.</param>
        /// <returns>The user role in a meeting, one of [Organizer, Presenter, Attendee].</returns>
        [HttpGet]
        [Route("me")]
        public async Task<ActionResult<string>> GetUserInformationAsync([FromQuery] string meetingId)
        {
            var (userId, tenantId) = this.HttpContext.GetUserAndTenantIds();
            if (!(meetingId.IsValid() && userId.IsValid() && tenantId.IsValid()))
            {
                return this.BadRequest("Missing or incorrect parameters");
            }

            // NOTE: The APIs in this controller get the user information from the access token in the Authorization header,
            // which the meeting tab obtained using tab SSO (https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso).
            // Do NOT rely on the user information in the tab context alone!
            var meetingRoleResponse = await this.meetingService.GetMeetingRoleAsync(meetingId, userId, tenantId);

            var userInfo = this.HttpContext.GetUserInfo(meetingRoleResponse.UserRole.Meeting.Role);
            return this.Ok(JsonConvert.SerializeObject(userInfo));
        }

        /// <summary>
        /// POST: Generate/Get the user token.
        /// </summary>
        /// <param name="meetingId">the meeting id.</param>
        /// <returns>A user token object.</returns>
        [HttpPost]
        [Route("me/token")]
        public async Task<ActionResult<string>> GenerateMeetingTokenAsync([FromQuery] string meetingId)
        {
            var (userId, tenantId) = this.HttpContext.GetUserAndTenantIds();
            if (!(meetingId.IsValid() && userId.IsValid() && tenantId.IsValid()))
            {
                return this.BadRequest("Missing or incorrect parameters");
            }

            // Get the user's role in the meeting
            // NOTE: This verifies that the user is actually a participant in the provided meeting.
            var meetingRoleResponse = await this.meetingService.GetMeetingRoleAsync(meetingId, userId, tenantId);

            var userInfo = this.HttpContext.GetUserInfo(meetingRoleResponse.UserRole.Meeting.Role);
            var userToken = await this.meetingTokenRepository.GenerateTokenAsync(meetingId, userInfo);
            return this.Ok(JsonConvert.SerializeObject(userToken, new StringEnumConverter()));
        }

        /// <summary>
        /// Acknowledge that a token is serviced.
        /// </summary>
        /// <param name="meetingId">the meeting id.</param>
        /// <returns>The status of the meeting after the acknowledging the token.</returns>
        [HttpPost]
        [Route("me/ack-token")]
        public async Task<ActionResult<string>> AcknowledgeTokenAsync([FromQuery] string meetingId)
        {
            var (userId, tenantId) = this.HttpContext.GetUserAndTenantIds();
            if (!(meetingId.IsValid() && userId.IsValid() && tenantId.IsValid()))
            {
                return this.BadRequest("Missing or incorrect parameters");
            }

            // Get the user's role in the meeting. This verifies that the user is actually a participant in the provided meeting.
            var meetingRoleResponse = await this.meetingService.GetMeetingRoleAsync(meetingId, userId, tenantId);

            var meetingStatus = await this.meetingTokenRepository.AcknowledgeTokenAsync(meetingId, userId);

            var currentUser = meetingStatus.UserTokens.Find(token => token.Status.Equals(TokenStatus.Current));
            if (currentUser != null)
            {
                // NOTE: Get the meeting chat conversation ID from the provided meeting ID, instead of taking it from the tab context.
                // This guards against a user modifying the meeting ID to a meeting that they are participating in (thereby passing the
                // membership and/or role check), but providing a different conversation ID that is not actually the meeting chat thread.
                await this.meetingService.PostStatusChangeNotification(
                    meetingRoleResponse.UserRole.Conversation.Id,
                    tenantId,
                    meetingStatus.MeetingMetadata.CurrentToken,
                    currentUser.UserInfo.Name);
            }

            return this.Ok(JsonConvert.SerializeObject(meetingStatus, new StringEnumConverter()));
        }

        /// <summary>
        /// Get the meeting summary.
        /// </summary>
        /// <param name="meetingId">the meeting id.</param>
        /// <returns>The meeting status.</returns>
        [HttpGet]
        [Route("meeting/summary")]
        public async Task<ActionResult<string>> GetMeetingSummaryAsync([FromQuery] string meetingId)
        {
            var (userId, tenantId) = this.HttpContext.GetUserAndTenantIds();
            if (!(meetingId.IsValid() && userId.IsValid() && tenantId.IsValid()))
            {
                return this.BadRequest("Missing or incorrect parameters");
            }

            // Get the user's role in the meeting. This verifies that the user is actually a participant in the provided meeting.
            await this.meetingService.GetMeetingRoleAsync(meetingId, userId, tenantId);

            var meetingSummary = await this.meetingTokenRepository.GetMeetingSummaryAsync(meetingId);
            return this.Ok(JsonConvert.SerializeObject(meetingSummary, new StringEnumConverter()));
        }

        /// <summary>
        /// Skip the current user's token.
        /// </summary>
        /// <param name="meetingId">the meeting id.</param>
        /// <returns>The meeting status.</returns>
        [HttpPost]
        [Route("user/skip")]
        public async Task<ActionResult<string>> SkipCurrentUserAsync([FromQuery] string meetingId)
        {
            var (userId, tenantId) = this.HttpContext.GetUserAndTenantIds();

            if (!(meetingId.IsValid() && userId.IsValid() && tenantId.IsValid()))
            {
                return this.BadRequest("Missing or incorrect parameters");
            }

            // Get the user's role in the meeting. This verifies that the user is actually a participant in the provided meeting.
            var meetingRoleResponse = await this.meetingService.GetMeetingRoleAsync(meetingId, userId, tenantId);

            // Only organizers can skip the user with the token
            // NOTE: If your tab is designed to restrict some actions to specific roles, it's important to do the role check
            // in the API action itself, in addition to customizing the tab UX for each role.
            if (meetingRoleResponse.UserRole.Meeting.Role != Constants.UserRoles.Organizer)
            {
                return this.Forbid("Only meeting organizers can skip over the user with the current token");
            }

            var meetingStatus = await this.meetingTokenRepository.SkipTokenAsync(meetingId);

            var currentUser = meetingStatus.UserTokens.Find(token => token.Status.Equals(TokenStatus.Current));
            if (currentUser != null)
            {
                await this.meetingService.PostStatusChangeNotification(
                    meetingRoleResponse.UserRole.Conversation.Id,
                    tenantId,
                    meetingStatus.MeetingMetadata.CurrentToken,
                    currentUser.UserInfo.Name);
            }

            return this.Ok(JsonConvert.SerializeObject(meetingStatus, new StringEnumConverter()));
        }
    }
}
