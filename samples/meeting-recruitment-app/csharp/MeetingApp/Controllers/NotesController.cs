using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using MeetingApp.Data.Models;
using MeetingApp.Data.Repositories.Notes;
using MeetingApp.Model;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Controllers
{
    /// <summary>
    /// Class for Notes.
    /// </summary>
    [Route("api/Notes")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private INotesRepository _notesRepository;

        // Dependency injected dictionary for storing Conversation Data that has roster and note details.
        private readonly ConcurrentDictionary<string, ConversationData> _conversationDataReference;

        public NotesController(INotesRepository notesRepository, ConcurrentDictionary<string, ConversationData> conversationDataReference)
        {
            _notesRepository = notesRepository;
            _conversationDataReference = conversationDataReference;
        }

        /// <summary>
        /// Method to save the note details.
        /// </summary>
        /// <param name="notesEntity">Contains Notes related proeprties</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveNoteDetails([FromBody] NotesEntity notesEntity)
        {
            try
            {
                // Getting stored conversation data reference.
                var currentRosterInfo = new ConversationData();
                _conversationDataReference.TryGetValue(notesEntity.MeetingId, out currentRosterInfo);

                if (currentRosterInfo != null && currentRosterInfo.Roster != null && currentRosterInfo.Roster.Any(entity => entity.Email == notesEntity.AddedBy))
                {
                    notesEntity.AddedByName = currentRosterInfo.Roster.Length > 0 ? currentRosterInfo.Roster.Where(entity => entity.Email == notesEntity.AddedBy).FirstOrDefault().Name : "Unknown";
                }
                else
                {
                    notesEntity.AddedByName = notesEntity.AddedBy;
                }
                var result = await this._notesRepository.StoreOrUpdateQuestionEntityAsync(notesEntity);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to get notes by email
        /// </summary>
        /// <param name="email">Email of the current candidate</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetNotes(string email)
        {
            try
            {
                var notesDetail = await this._notesRepository.GetNotes(email);
                if (notesDetail == null) return NotFound();
                return Ok(notesDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
