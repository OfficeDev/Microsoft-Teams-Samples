using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Repositories.Questions
{
    /// <summary>
    /// Interface for table operations for Questions table
    /// </summary>
    public interface IQuestionsRepository
    {
        /// <summary>
        /// Store questions in table storage.
        /// </summary>
        /// <param name="entity">Represents questionSet entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        Task<bool> StoreQuestionEntityAsync(List<QuestionSetEntity> questionsSet);

        /// <summary>
        /// Update question in table storage.
        /// </summary>
        /// <param name="entity">Represents questionSet entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        Task<TableResult> UpdateQuestionEntityAsync(QuestionSetEntity entity);

        /// <summary>
        /// Get questions from table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<IEnumerable<QuestionSetEntity>> GetQuestions(string meetingId);

        /// <summary>
        /// Delete a particular question in table storage.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<int> DeleteQuestion(QuestionSetEntity entity);
    }
}
