namespace TeamsTalentMgmtApp.Models.DatabaseContext
{
    public sealed class Comment
    {
        public int CommentId { get; set; }

        public int CandidateId { get; set; }

        public string Text { get; set; }

        public string AuthorName { get; set; }

        public string AuthorRole { get; set; }

        public string AuthorProfilePicture { get; set; }
    }
}
