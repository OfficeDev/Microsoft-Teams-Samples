using System;
using System.Collections.Generic;

namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class Position
    {
        public Position()
        {
            Candidates = new List<Candidate>();
        }

        public int PositionId { get; set; }

        public string PositionExternalId { get; set; } =
            Guid.NewGuid().ToString("n").Substring(0, 8).ToUpperInvariant();

        public string Title { get; set; }

        public int DaysOpen { get; set; }

        public int Level { get; set; }

        public string Description { get; set; }

        public int HiringManagerId { get; set; }

        public Recruiter HiringManager { get; set; }

        public int LocationId { get; set; }

        public Location Location { get; set; }

        public List<Candidate> Candidates { get; set; }
    }
}
