using System.Runtime.Serialization;

namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public enum RecruiterRole
    {
        [EnumMember(Value = "Hiring manager")]
        HiringManager,

        [EnumMember(Value = "HR Staff")]
        HRStaff,

        [EnumMember(Value = "Interviewer")]
        Interviewer
    }
}