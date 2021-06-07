// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;

namespace Microsoft.Teams.Apps.VirtualAssistant.Controllers
{
    /// <summary>
    /// A controller that handles skill replies to the bot.
    /// This example uses the <see cref="SkillHandler"/> that is registered as a <see cref="ChannelServiceHandler"/> in startup.cs.
    /// </summary>
    [Route("api/skills")]
    [ApiController]
    public class SkillController : ChannelServiceController
    {
        private ChannelServiceHandler _handler;

        public SkillController(ChannelServiceHandler handler)
            : base(handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// ReplyToActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL).</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("v3/conversations/{conversationId}/activities/{activityId}")]
        public override async Task<IActionResult> ReplyToActivityAsync(string conversationId, string activityId, [FromBody] Activity activity)
        {
            var result = await _handler.HandleReplyToActivityAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId, activity).ConfigureAwait(false);
            return new JsonResult(result);
        }

        /// <summary>
        /// SendToConversation.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("v3/conversations/{conversationId}/activities")]
        public override async Task<IActionResult> SendToConversationAsync(string conversationId, [FromBody] Activity activity)
        {
            var result = await _handler.HandleSendToConversationAsync(HttpContext.Request.Headers["Authorization"], conversationId, activity).ConfigureAwait(false);
            return new JsonResult(result);
        }
    }
}