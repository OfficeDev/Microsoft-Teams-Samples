using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace MeetingApp.Data.Models
{
    public class CandidateDetailEntity: TableEntity
    {
        public string CandidateName { get; set; }

        public string Role { get; set; }

        public string Experience { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string Skills { get; set; }

        public string Source { get; set; }

        public string Attachments { get; set; }
    }
}
