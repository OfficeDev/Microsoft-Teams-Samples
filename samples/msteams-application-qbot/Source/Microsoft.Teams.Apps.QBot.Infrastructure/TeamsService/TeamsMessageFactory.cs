namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Infrastructure;

    /// <summary>
    /// Message factory implementation.
    /// </summary>
    internal sealed class TeamsMessageFactory : IMessageFactory
    {
        /// <summary>
        /// Select answer adaptive card template file path.
        /// {0} - base directory.
        /// </summary>
        private const string ErrorCardTemplatePath = "{0}\\assets\\templates\\error-card-template.json";

        /// <summary>
        /// Select answer adaptive card template file path.
        /// {0} - base directory.
        /// </summary>
        private const string SelectAnswerTemplatePath = "{0}\\assets\\templates\\select-answer-template.json";

        /// <summary>
        /// Correct answer adaptive card template file path.
        /// {0} - base directory.
        /// </summary>
        private const string CorrectAnswerTemplatePath = "{0}\\assets\\templates\\correct-answer-template.json";

        /// <summary>
        /// Suggest answer adaptive card template file path.
        /// {0} - base directory.
        /// </summary>
        private const string SuggestAnswerTemplatePath = "{0}\\assets\\templates\\suggest-answer-template.json";

        /// <summary>
        /// Helpful answer adaptive card template file path.
        /// {0} - base directory.
        /// </summary>
        private const string HelpfulAnswerTemplatePath = "{0}\\assets\\templates\\helpful-answer-template.json";

        /// <summary>
        /// Not Helpful answer adaptive card template file path.
        /// {0} - base directory.
        /// </summary>
        private const string NotHelpfulAnswerTemplatePath = "{0}\\assets\\templates\\nothelpful-answer-template.json";

        /// <summary>
        /// Question answered adaptive card template.
        /// {0} - base directory.
        /// </summary>
        private const string QuestionAnsweredTemplatePath = "{0}\\assets\\templates\\question-answered-template.json";

        /// <summary>
        /// Create new thread adaptive card template file path..
        /// {0} - baseUrl.
        /// </summary>
        private const string CreateNewThreadTemplatePath = "{0}\\assets\\templates\\create-new-thread.json";

        /// <summary>
        /// Default profile pic for a user.
        /// {0} - baseUrl.
        /// </summary>
        private const string DefaultProfilePicUrl = "{0}/images/default-profile-pic.png";

        /// <summary>
        /// Bot profile pic.
        /// {0} - baseUrl.
        /// </summary>
        private const string BotProfilePicUrl = "{0}/images/bot-profile-pic.png";

        /// <summary>
        /// Base64 encoded image url in adaptive cards.
        /// {0} base64 encoded image.
        /// </summary>
        private const string Base64EncodedImageUrl = "data:image/png;base64,{0}";
        private readonly IAppSettings appSettings;
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMessageFactory"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        /// <param name="localizer">Localizer.</param>
        public TeamsMessageFactory(
            IAppSettings appSettings,
            IStringLocalizer<Strings> localizer)
        {
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        /// <inheritdoc/>
        public Attachment CreateAnswerMessage(Question question, Answer answer, IEnumerable<User> users)
        {
            // If Bot's answer is selected.
            if (answer.AuthorId.Contains(this.appSettings.BotAppId))
            {
                return this.CreateHelpfulAnswer(question, answer, users);
            }

            // else Post Correct Answer Template.
            var acceptedBy = users.FirstOrDefault(user => user.AadId == answer.AcceptedById);

            var answerAuthor = users.FirstOrDefault(user => user.AadId == answer.AuthorId);
            var authorProfileImage = this.GetUserProfilePic(answerAuthor);
            var data = new
            {
                answer = new
                {
                    authorName = answerAuthor.Name,
                    authorProfileImage = authorProfileImage,
                    message = answer.Message,
                    acceptedByUserName = acceptedBy.Name,
                },
                text = new
                {
                    answeredBy = this.localizer.GetString("answeredBy").Value,
                    answeredBySummary = this.localizer.GetString("answeredBySummary", acceptedBy.Name).Value,
                },
            };

            var template = this.GetCardTemplate(CorrectAnswerTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment CreateSelectAnswerMessage(Question question, IEnumerable<User> users)
        {
            var tags = users.Select(user => this.GetAtMentionText(user.Name));
            var message = string.Join(" ", tags);
            message = this.localizer.GetString("tagEducators", message).Value;
            var data = new
            {
                message,
                questionId = question.Id,
                selectAnswer = this.localizer.GetString("selectAnswer").Value,
            };

            var mentions = users.Select(user => new Mention()
            {
                Mentioned = new ChannelAccount()
                {
                    AadObjectId = user.AadId,
                    Id = user.TeamId,
                    Name = user.Name,
                },
                Text = this.GetAtMentionText(user.Name),
            });
            var entities = new { entities = new List<Entity>(mentions) };

            var template = this.GetCardTemplate(SelectAnswerTemplatePath);
            var serializedJson = template.Expand(data);
            var card = AdaptiveCard.FromJson(serializedJson).Card;

            card.AdditionalProperties.Add("msteams", entities);
            return this.CreateAttachment(card.ToJson());
        }

        /// <inheritdoc/>
        public Attachment CreateSuggestAnswerMessage(Question question, SuggestedAnswer suggestedAnswer)
        {
            var data = new
            {
                suggestedAnswer = new
                {
                    id = suggestedAnswer.Id,
                    message = suggestedAnswer.Answer,
                },
                question = new
                {
                    id = question.Id,
                },
                text = new
                {
                    helpful = this.localizer.GetString("helpful").Value,
                    notHelpful = this.localizer.GetString("notHelpful").Value,
                    confidence = this.localizer.GetString("confidence", suggestedAnswer.Score).Value,
                },
            };

            var template = this.GetCardTemplate(SuggestAnswerTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment CreateNotHelpfulMessage(Answer answer, User user)
        {
            var data = new
            {
                answer = new
                {
                    message = answer.Message,
                    acceptedBy = this.localizer.GetString("nothelpfulAcceptedBy", user.Name).Value,
                },
                questionId = answer.QuestionId,
                selectAnswer = this.localizer.GetString("selectAnswer").Value,
            };

            var template = this.GetCardTemplate(NotHelpfulAnswerTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment CreateErrorMessage(string errorMessage)
        {
            var data = new
            {
                error = new
                {
                    message = errorMessage,
                },
            };

            var template = this.GetCardTemplate(ErrorCardTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment CreateQuestionAnsweredMessage(string deepLink)
        {
            var data = new
            {
                message = this.localizer.GetString("questionIsAnsweredMessage").Value,
                buttonTitle = this.localizer.GetString("goToAnswerButtonTitle").Value,
                buttonUrl = deepLink,
            };

            var template = this.GetCardTemplate(QuestionAnsweredTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        /// <inheritdoc/>
        public Attachment CreateNewThreadMessage()
        {
            var url = this.localizer.GetString("createNewThreadGIFUrl", this.appSettings.BaseUrl).Value;
            var data = new
            {
                message = this.localizer.GetString("createNewThreadMessage").Value,
                imageUrl = url,
            };

            var template = this.GetCardTemplate(CreateNewThreadTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        private AdaptiveCardTemplate GetCardTemplate(string templatePath)
        {
            templatePath = string.Format(templatePath, AppDomain.CurrentDomain.BaseDirectory);
            return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
        }

        private Attachment CreateAttachment(string adaptiveCardJson)
        {
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard.Card,
            };
        }

        private string GetAtMentionText(string userName)
        {
            return $"<at>{userName}</at>";
        }

        private Attachment CreateHelpfulAnswer(Question question, Answer answer, IEnumerable<User> users)
        {
            var acceptedBy = users.FirstOrDefault(user => user.AadId == answer.AcceptedById);

            var data = new
            {
                answer = new
                {
                    authorName = this.appSettings.BotName,
                    authorProfileImage = string.Format(BotProfilePicUrl, this.appSettings.BaseUrl),
                    message = answer.Message,
                    acceptedBy = this.localizer.GetString("helpfulAcceptedBy", acceptedBy.Name).Value,
                },
                text = new
                {
                    answeredBy = this.localizer.GetString("answeredBy").Value,
                },
            };

            var template = this.GetCardTemplate(HelpfulAnswerTemplatePath);
            var serializedJson = template.Expand(data);
            return this.CreateAttachment(serializedJson);
        }

        private string GetUserProfilePic(User user)
        {
            // If user's profile pic isn't set.
            if (string.IsNullOrEmpty(user.ProfilePicUrl))
            {
                return string.Format(DefaultProfilePicUrl, this.appSettings.BaseUrl);
            }

            return this.GetBase64EncodedImageUrl(user.ProfilePicUrl);
        }

        private string GetBase64EncodedImageUrl(string dataUri)
        {
            return string.Format(Base64EncodedImageUrl, dataUri);
        }
    }
}
