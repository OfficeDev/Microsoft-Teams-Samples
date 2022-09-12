// <copyright file="QBotTeamInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Infrastructure;

    /// <summary>
    /// Fetches <see cref="Course"/>, <see cref="Channel"/> and <see cref="Member"/> information from Teams Models.
    /// </summary>
    internal class QBotTeamInfo : IQBotTeamInfo
    {
        private readonly ITeamInfoService teamInfoService;
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="QBotTeamInfo"/> class.
        /// </summary>
        /// <param name="teamInfoService">Team info service.</param>
        /// <param name="localizer">Lozalizer.</param>
        public QBotTeamInfo(
            ITeamInfoService teamInfoService,
            IStringLocalizer<Strings> localizer)
        {
            this.teamInfoService = teamInfoService ?? throw new System.ArgumentNullException(nameof(teamInfoService));
            this.localizer = localizer ?? throw new System.ArgumentNullException(nameof(localizer));
        }

        /// <inheritdoc/>
        public async Task<Course> GetCourseAsync(TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, teamInfo.Id, cancellationToken);
            return new Course()
            {
                Id = teamDetails.Id,
                TeamId = teamDetails.Id,
                TeamAadObjectId = teamDetails.AadGroupId,
                Name = teamDetails.Name,
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Channel>> GetChannelsAsync(TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var teamChannels = await TeamsInfo.GetTeamChannelsAsync(turnContext, teamInfo.Id, cancellationToken);
            return teamChannels.Select(channel => this.ConvertToChannelInternal(channel, teamInfo));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Member>> GetCourseMembersAsync(TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Note: We do not filter guest users.
            var members = new List<TeamsChannelAccount>();
            string continuationToken = null;

            do
            {
                var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 500/*pageSize*/, continuationToken, cancellationToken);
                continuationToken = currentPage.ContinuationToken;
                members = members.Concat(currentPage.Members).ToList();
            }
            while (continuationToken != null);

            var owners = await this.teamInfoService.GetTeamOwnersIdsAsync(teamInfo.AadGroupId);
            return members
                .Select(member => this.ConvertToMember(member, owners.ToHashSet()));
        }

        /// <inheritdoc/>
        public IEnumerable<Member> ConvertToCourseMembers(IList<TeamsChannelAccount> teamsMembers, TeamInfo teamInfo, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return teamsMembers.Select(member => this.ConvertToMember(member, new HashSet<string>()));
        }

        /// <inheritdoc/>
        public Channel ConvertToChannel(ChannelInfo channelInfo, TeamInfo teamInfo)
        {
            return this.ConvertToChannelInternal(channelInfo, teamInfo);
        }

        /// <inheritdoc/>
        public Task<Question> GetQuestionAsync(TeamInfo teamInfo, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            Question question = new Question()
            {
                AuthorId = activity.From.AadObjectId,
                ChannelId = activity.TeamsGetChannelId(),
                CourseId = teamInfo.Id,
                MessageId = activity.Conversation.Id.Split('=')[1],
                Message = activity.Text,
                TimeStamp = activity.Timestamp.Value,
            };

            return Task.FromResult(question);
        }

        #region Private Methods

        private Channel ConvertToChannelInternal(ChannelInfo channel, TeamInfo teamInfo)
        {
            return new Channel()
            {
                Id = channel.Id,
                Name = channel.Id == teamInfo.Id ? this.localizer.GetString("generalChannelName") : channel.Name,
                CourseId = teamInfo.Id,
            };
        }

        private Member ConvertToMember(TeamsChannelAccount member, ISet<string> owners)
        {
            return new Member()
            {
                TeamId = member.Id,
                AadId = member.AadObjectId,
                Name = member.Name,
                Role = owners.Contains(member.AadObjectId) ? MemberRole.Educator : MemberRole.Student,
                Upn = member.UserPrincipalName,
            };
        }

        #endregion
    }
}
