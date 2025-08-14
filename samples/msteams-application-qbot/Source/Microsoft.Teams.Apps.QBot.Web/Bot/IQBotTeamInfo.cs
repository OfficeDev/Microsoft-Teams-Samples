// <copyright file="IQBotTeamInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Defines contract to read QBot models from teams bot events
    /// </summary>
    public interface IQBotTeamInfo
    {
        /// <summary>
        /// Gets <see cref="Course"/> from teamInfo.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Course> GetCourseAsync(TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a list of <see cref="Channel"/> in the course.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<IEnumerable<Channel>> GetChannelsAsync(TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a list of <see cref="Member"/> in the course.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<IEnumerable<Member>> GetCourseMembersAsync(TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Prepares <see cref="Question"/> from the message acitivity.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns><see cref="Question"/>.</returns>
        public Task<Question> GetQuestionAsync(TeamInfo teamInfo, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Converts to a list of <see cref="Member"/>.
        /// </summary>
        /// <param name="teamsMembers">A list of team members.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of <see cref="Member"/>.</returns>
        public IEnumerable<Member> ConvertToCourseMembers(IList<TeamsChannelAccount> teamsMembers, TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Converts <see cref="TeamsChannelAccount"/> to <see cref="Channel"/>.
        /// </summary>
        /// <param name="channelInfo">Channel info.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <returns>Channel.</returns>
        public Channel ConvertToChannel(ChannelInfo channelInfo, TeamInfo teamInfo);
    }
}
