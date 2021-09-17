using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Repositories.Questions
{
    public interface IQuestionsRepository
    {
        Task<TableResult> StoreOrUpdateQuestionEntityAsync(QuestionSetEntity entity);

        Task<IEnumerable<QuestionSetEntity>> GetQuestions(string meetingId);

        Task<TableResult> DeleteQuestion(QuestionSetEntity entity);
    }
}
