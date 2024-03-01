using System.Collections.Generic;

namespace TeamsTalentMgmtApp.Models.Dto
{
    public sealed class CandidateDto
    {
        public CandidateDto()
        {
            Interviews = new List<InterviewDto>();
            Comments = new List<CommentDto>();
        }

        public int CandidateId { get; set; }

        public string Name { get; set; }

        public string PositionTitle { get; set; }

        public string Stage { get; set; }

        public string CurrentRole { get; set; }

        public string Phone { get; set; }

        public string ProfilePicture { get; set; }

        public string ProfilePictureDataOnly => ProfilePicture.Replace("data:image/png;base64,", string.Empty);

        public int PositionId { get; set; }

        public LocationDto Location { get; set; }

        public List<InterviewDto> Interviews { get; set; }

        public List<CommentDto> Comments { get; set; }
    }
}
