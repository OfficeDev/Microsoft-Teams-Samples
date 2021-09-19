using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Models
{
    public class NotesEntity: TableEntity
    {
        public string CandidateEmail { get; set; }

        public string AddedBy { get; set; }

        public string Note { get; set; }
    }
}
