using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Models
{
    /// <summary>
    /// Feedback related properties.
    /// </summary>
    public class FeedbackEntity: TableEntity
    {
        public string MeetingId { get; set; }

        public string CandidateEmail { get; set; }

        public string FeedbackJson { get; set; }

        public string Interviewer { get; set; }
    }
}
