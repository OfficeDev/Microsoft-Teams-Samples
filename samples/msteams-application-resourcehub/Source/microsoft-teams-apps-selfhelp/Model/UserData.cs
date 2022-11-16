namespace Microsoft.Teams.Selfhelp.Authentication.Model
{
    /// <summary>
    /// Teams data model class.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Gets or sets user id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets image.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets job title.
        /// </summary>
        public string JobTitle { get; set; }
    }
}