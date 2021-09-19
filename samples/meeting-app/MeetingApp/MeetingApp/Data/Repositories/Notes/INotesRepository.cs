using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Repositories.Notes
{
    public interface INotesRepository
    {
        Task<TableResult> StoreOrUpdateQuestionEntityAsync(NotesEntity entity);

        Task<IEnumerable<NotesEntity>> GetNotes(string email);
    }
}
