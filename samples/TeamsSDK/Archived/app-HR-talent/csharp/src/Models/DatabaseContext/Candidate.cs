using System;
using System.Collections.Generic;

namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class Candidate
    {
        public Candidate()
        {
            Comments = new List<Comment>();
            Interviews = new List<Interview>();
        }

        public int CandidateId { get; set; }

        public string Name { get; set; }

        public InterviewStageType Stage { get; set; }

        public InterviewStageType PreviousStage { get; set; }

        public string Phone { get; set; }

        public string CurrentRole { get; set; }

        public string ProfilePicture { get; set; }

        public string Summary { get; set; }

        public DateTime DateApplied { get; set; }

        public List<Comment> Comments { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; }

        public List<Interview> Interviews { get; set; }
    }
}
