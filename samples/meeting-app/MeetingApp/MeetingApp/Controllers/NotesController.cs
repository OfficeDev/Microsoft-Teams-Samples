using MeetingApp.Data.Models;
using MeetingApp.Data.Repositories.Notes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Controllers
{
    [Route("api/Notes")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private INotesRepository _notesRepository;

        public NotesController(INotesRepository notesRepository)
        {
            _notesRepository = notesRepository;
        }

        [HttpPost]
        public async Task<IActionResult> SaveNoteDetails([FromBody]NotesEntity notesEntity)
        {
            var result = await this._notesRepository.StoreOrUpdateQuestionEntityAsync(notesEntity);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes(string email)
        {
            var questions = await this._notesRepository.GetNotes(email);
            if (questions == null) return NotFound();
            return Ok(questions);
        }
    }
}
