namespace Microsoft.Teams.Apps.QBot.Infrastructure.QnAService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// QnA Service implementation.
    /// </summary>
    internal class QnAService : IQnAService
    {
        private const string LogicalKnowledgeBaseName = "LogicalKB";

        private readonly IQnAMakerClient qnAClient;
        private readonly IQnAMakerRuntimeClient qnARuntimeClient;
        private readonly IQnAServiceSettings settings;
        private readonly ILogger<QnAService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAService"/> class.
        /// </summary>
        /// <param name="qnAClient">QnA Maker client.</param>
        /// <param name="qnARuntimeClient">QnA Maker runtime client.</param>
        /// <param name="settings">QnA Service settings.</param>
        /// <param name="logger">Logger.</param>
        public QnAService(
            IQnAMakerClient qnAClient,
            IQnAMakerRuntimeClient qnARuntimeClient,
            IQnAServiceSettings settings,
            ILogger<QnAService> logger)
        {
            this.qnAClient = qnAClient ?? throw new ArgumentNullException(nameof(qnAClient));
            this.qnARuntimeClient = qnARuntimeClient ?? throw new ArgumentNullException(nameof(qnARuntimeClient));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<SuggestedAnswer> GetAnswerAsync(Question question, string logicalKnowledgeBaseId)
        {
            var query = new QueryDTO()
            {
                Question = question.GetSanitizedMessage(),
                ScoreThreshold = this.settings.ScoreThreshold,
                StrictFilters = this.GetMetaData(logicalKnowledgeBaseId),
                IsTest = false,
            };

            try
            {
                var response = await this.qnARuntimeClient.Runtime.GenerateAnswerAsync(this.settings.KnowledgeBaseId, query);
                var searchResult = response.Answers.FirstOrDefault();
                return new SuggestedAnswer()
                {
                    Id = searchResult.Id.Value,
                    Answer = searchResult.Answer,
                    Score = (int)searchResult.Score,
                };
            }
            catch (ErrorResponseException exception)
            {
                var message = $"Failed to generate answer from QnA Service. Question Id: {question.Id}.";
                this.logger.LogWarning(exception, message);

                if (exception.Response?.StatusCode == HttpStatusCode.BadRequest)
                {
                    // This is expected when there is no data with the logical Kb key.
                    // We could do another check on response content, but didn't find any documentation that it would stay the way it is today.
                    this.logger.LogWarning($"BadRequest: Response Content: {exception.Response.Content}");
                    return new SuggestedAnswer()
                    {
                        Answer = string.Empty,
                        Score = 0,
                    };
                }

                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task<string> PostQnAPairAsync(Question question, Answer answer, string logicalKnowledgeBaseId)
        {
            var qnADTO = new QnADTO()
            {
                Questions = new List<string>() { question.GetSanitizedMessage() },
                Answer = answer.GetSanitizedMessage(),
                Metadata = this.GetMetaData(logicalKnowledgeBaseId),
            };

            var operation = new UpdateKbOperationDTO()
            {
                Add = new UpdateKbOperationDTOAdd()
                {
                    QnaList = new List<QnADTO>() { qnADTO },
                },
            };

            try
            {
                var response = await this.qnAClient.Knowledgebase.UpdateAsync(this.settings.KnowledgeBaseId, operation);
                return response.OperationId; // Note: We do not check operation status.
            }
            catch (ErrorResponseException exception)
            {
                var message = $"Failed to post QnA Pair to QnA Service. Question Id: {question.Id}. Answer Id: {answer.Id}";
                this.logger.LogWarning(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task PublishKbAsync()
        {
            try
            {
                await this.qnAClient.Knowledgebase.PublishAsync(this.settings.KnowledgeBaseId);
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Failed to publish KB");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Failed to publish Kb.");
            }
        }

        /// <inheritdoc/>
        public async Task UpdateQnAPairAsync(Question question, SuggestedAnswer suggestedAnswer)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (suggestedAnswer is null)
            {
                throw new ArgumentNullException(nameof(suggestedAnswer));
            }

            var dto = new UpdateQnaDTO()
            {
                Id = suggestedAnswer.Id,
                Questions = new UpdateQnaDTOQuestions()
                {
                    Add = new List<string>() { question.GetSanitizedMessage() },
                },
            };

            var operation = new UpdateKbOperationDTO()
            {
                Update = new UpdateKbOperationDTOUpdate()
                {
                    QnaList = new List<UpdateQnaDTO>() { dto },
                },
            };

            try
            {
                var response = await this.qnAClient.Knowledgebase.UpdateAsync(this.settings.KnowledgeBaseId, operation);
            }
            catch (ErrorResponseException exception)
            {
                var message = $"Failed to update QnA Pair to QnA Service. Question Id: {question.Id}, QnAPair Id : {suggestedAnswer.Id}.";
                this.logger.LogWarning(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        private List<MetadataDTO> GetMetaData(string logicalKnowledgeBaseValue)
        {
            var metaData = new MetadataDTO()
            {
                Name = LogicalKnowledgeBaseName,
                Value = logicalKnowledgeBaseValue,
            };

            return new List<MetadataDTO>() { metaData };
        }
    }
}
