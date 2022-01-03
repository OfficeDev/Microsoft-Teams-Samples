using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    [Route("api/Send")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private INotesRepository _notesRepository;

        // Dependency injected dictionary for storing Conversation Data that has roster and note details.
        private readonly ConcurrentDictionary<string, ConversationData> _conversationDataReference;

        public CardController(INotesRepository notesRepository, ConcurrentDictionary<string, ConversationData> conversationDataReference)
        {
            _notesRepository = notesRepository;
            _conversationDataReference = conversationDataReference;
        }

        /// <summary>
        /// Method to save the note details.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveNoteDetails([FromBody] CardEntity cardEntity)
        {
            try
            {
                var card = cardEntity.CardBody;
                var webhook = "https://m365x881152.webhook.office.com/webhookb2/c980b7ba-9c35-49c8-9439-3b41800a9b46@c9f9aafd-64ac-4f38-8e05-12feba3fb090/IncomingWebhook/035ed0e18b3e438fa9c8a4b4099dc75c/ef16aa89-5b26-4a2c-aebb-761b551577c0";
                string cardJson = @"{
                ""@type"": ""MessageCard"",
                ""summary"": ""Welcome Message"",
                ""sections"": [{ 
                ""activityTitle"": ""Welcome Message"",
                ""text"": ""Teams ToDo connector has been set up""}]}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(cardEntity.CardBody, System.Text.Encoding.UTF8, "application/json");
                using (var response = await client.PostAsync(webhook, content))
                {
                    // Check response.IsSuccessStatusCode and take appropriate action if needed.
                }
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
