namespace Microsoft.Teams.Apps.QBot.Domain.IServices
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// User srvice contract.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets user's profile.
        /// </summary>
        /// <param name="userId">User's AAD object id.</param>
        /// <param name="fetchPhoto">If it should fetch user's photo.</param>
        /// <returns>User profile.</returns>
        Task<User> GetUserProfileAsync(string userId, bool fetchPhoto = false);

        /// <summary>
        /// Gets user's profile pic.
        /// </summary>
        /// <param name="userId">User's AAD id.</param>
        /// <returns>User's profile pic.</returns>
        Task<string> GetUserProfilePicAsync(string userId);
    }
}
