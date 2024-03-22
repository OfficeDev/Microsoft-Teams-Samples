using System.Collections.ObjectModel;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Models.TemplateModels
{
    public class CandidateTemplateModel : BaseTemplateModel<Candidate>
    {
        public ReadOnlyCollection<Recruiter> Interviewers { get; set; }

        public AppSettings AppSettings { get; set; }

        public string Locale { get; set; }
    }
}
