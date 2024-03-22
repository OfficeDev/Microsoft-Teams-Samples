using System;

namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class Interview
    {
        public int InterviewId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string FeedbackText { get; set; }

        public int CandidateId { get; set; }

        public int RecruiterId { get; set; }

        public Recruiter Recruiter { get; set; }
    }
}
