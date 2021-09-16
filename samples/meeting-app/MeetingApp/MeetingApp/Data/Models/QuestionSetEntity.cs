using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Models
{
    public class QuestionSetEntity: TableEntity
    {
        public string MeetingId { get; set; }

        public string Question { get; set; }

        public string SetBy { get; set; }

        public string IsDelete { get; set; }
    }
}
