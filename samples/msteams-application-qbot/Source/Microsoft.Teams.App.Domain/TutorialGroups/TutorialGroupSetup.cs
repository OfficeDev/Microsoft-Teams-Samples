namespace Microsoft.Teams.Apps.QBot.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// TutorialGroup set-up.
    /// </summary>
    internal sealed class TutorialGroupSetup : ITutorialGroupSetup
    {
        private readonly ITutorialGroupValidator tutorialGroupValidator;
        private readonly IMemberRepository memberRepository;
        private readonly ITutorialGroupRepository tutorialGroupRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TutorialGroupSetup"/> class.
        /// </summary>
        /// <param name="tutorialGroupValidator">Validator.</param>
        /// <param name="memberRepository">Member reposiroty.</param>
        /// <param name="tutorialGroupRepository">Tutorial group repository.</param>
        public TutorialGroupSetup(
            ITutorialGroupValidator tutorialGroupValidator,
            IMemberRepository memberRepository,
            ITutorialGroupRepository tutorialGroupRepository)
        {
            this.tutorialGroupValidator = tutorialGroupValidator ?? throw new ArgumentNullException(nameof(tutorialGroupValidator));
            this.memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            this.tutorialGroupRepository = tutorialGroupRepository ?? throw new ArgumentNullException(nameof(tutorialGroupRepository));
        }

        /// <inheritdoc/>
        public async Task UpdateTutorialGroupAsync(TutorialGroup tutorialGroup)
        {
            if (!this.tutorialGroupValidator.IsValid(tutorialGroup))
            {
                throw new QBotException(System.Net.HttpStatusCode.BadRequest, ErrorCode.InvalidTutorialGroup, "Invalid tutorial group definition.");
            }

            await this.tutorialGroupRepository.UpdateTutorialGroupAsync(tutorialGroup);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Member>> GetAllTutorialGroupMembersAsync(string tutorialGroupId)
        {
            if (string.IsNullOrEmpty(tutorialGroupId))
            {
                throw new System.ArgumentException($"'{nameof(tutorialGroupId)}' cannot be null or empty", nameof(tutorialGroupId));
            }

            return this.memberRepository.GetTutorialGroupMembersAsync(tutorialGroupId);
        }

        /// <inheritdoc/>
        public Task<TutorialGroup> GetTutorialGroupAsync(string tutorialGroupId)
        {
            if (string.IsNullOrEmpty(tutorialGroupId))
            {
                throw new ArgumentException($"'{nameof(tutorialGroupId)}' cannot be null or empty.", nameof(tutorialGroupId));
            }

            return this.tutorialGroupRepository.GetTutorialGroupAsync(tutorialGroupId);
        }
    }
}
