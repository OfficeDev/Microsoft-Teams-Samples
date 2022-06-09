using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace MeetingTranscription.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConcurrentDictionary<string, string> transcriptsDictionary;

        public HomeController (ConcurrentDictionary<string, string> transcriptsDictionary)
        {
            this.transcriptsDictionary = transcriptsDictionary;
        }

        public IActionResult Index([FromQuery] string meetingId)
        {
            ViewBag.Transcripts = "Transcript not found.";

            if (meetingId != null)
            {
                var isFound = transcriptsDictionary.TryGetValue(meetingId, out string transcripts);
                if (isFound)
                {
                    ViewBag.Transcripts = transcripts;
                }
            }

            return View();
        }
    }
}
