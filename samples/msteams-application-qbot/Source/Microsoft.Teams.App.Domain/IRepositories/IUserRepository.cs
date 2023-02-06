namespace Microsoft.Teams.Apps.QBot.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// User repository interface.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Adds or updates user model to database.
        /// </summary>
        /// <param name="user">User model object.</param>
        /// <returns>async task.</returns>
        Task AddOrUpdateUserAsync(User user);

        /// <summary>
        /// Gets user profile from the database.
        /// </summary>
        /// <param name="userAadId">User's AAD id.</param>
        /// <returns>User's profile.</returns>
        Task<User> GetUserAsync(string userAadId);

        /// <summary>
        /// Gets users profile from the database.
        /// If user's profile is not found, it will not be returned.
        /// </summary>
        /// <param name="userAadIds">List of user AAD ids.</param>
        /// <returns>List of user profiles.</returns>
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userAadIds);

        /// <summary>
        /// Gets list of users who are not part of any course.
        /// </summary>
        /// <returns>list of user profiles.</returns>
        Task<IEnumerable<User>> GetUsersWithNoCourseAsync();

        /// <summary>
        /// Updates user profile in the database.
        /// </summary>
        /// <param name="updatedUser">Updated user profile.</param>
        /// <returns>Async tyask.</returns>
        Task UpdateUserAsync(User updatedUser);

        /// <summary>
        /// Deletes user profile from the database.
        /// </summary>
        /// <param name="userAadId">User id.</param>
        /// <returns>async task.</returns>
        Task DeleteUserAsync(string userAadId);
    }
}
