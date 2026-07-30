using System.Collections.Generic;

namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class Recruiter
    {
        public Recruiter()
        {
            Positions = new List<Position>();
        }

        public int RecruiterId { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }

        public string ProfilePicture { get; set; }

        public RecruiterRole Role { get; set; }

        public string DirectReportIds { get; set; }

        public List<Position> Positions { get; set; }

        public ConversationData ConversationData { get; set; }
    }
}
