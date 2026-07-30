using System;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Models.Dto
{
    public sealed class InterviewDto
    {
        public int InterviewId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string FeedbackText { get; set; }

        public int CandidateId { get; set; }

        public int RecruiterId { get; set; }

        public Recruiter Recruiter { get; set; }
    }
}
