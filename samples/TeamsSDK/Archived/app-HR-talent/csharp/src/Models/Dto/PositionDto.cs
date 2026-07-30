using System.Collections.Generic;

namespace TeamsTalentMgmtApp.Models.Dto
{
    public sealed class PositionDto
    {
        public PositionDto()
        {
            Candidates = new List<CandidateDto>();
        }

        public const int MaxDescriptionLength = 300;

        public int PositionId { get; set; }

        public string PositionExternalId { get; set; }

        public string Title { get; set; }

        public int DaysOpen { get; set; }

        public string Description => FullDescription?.Length > MaxDescriptionLength ? FullDescription.Substring(0, MaxDescriptionLength) + "..." : FullDescription;

        public string FullDescription { get; set; }

        public ICollection<CandidateDto> Candidates { get; set; }

        public RecruiterDto HiringManager { get; set; }

        public LocationDto Location { get; set; }
    }
}
