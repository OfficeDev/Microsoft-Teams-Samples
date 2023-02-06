namespace Microsoft.Teams.Apps.QBot.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// <see cref="Channel"/> repository.
    /// </summary>
    public interface IChannelRepository
    {
        /// <summary>
        /// Add the channel to db.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <returns>async task.</returns>
        Task AddChannelAsync(Channel channel);

        /// <summary>
        /// Adds list of channels to db.
        /// </summary>
        /// <param name="channels">Channels.</param>
        /// <returns>Async task.</returns>
        Task AddChannelsAsync(IEnumerable<Channel> channels);

        /// <summary>
        /// Updates channel.
        /// </summary>
        /// <param name="channel">Updated channel object.</param>
        /// <returns>async task.</returns>
        Task UpdateChannelAsync(Channel channel);

        /// <summary>
        /// Deletes channel.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Async task.</returns>
        Task DeleteChannelAsync(string channelId);

        /// <summary>
        /// Gets channel.
        /// </summary>
        /// <param name="channelId">Channel id.</param>
        /// <returns>Channel.</returns>
        Task<Channel> GetChannelAsync(string channelId);

        /// <summary>
        /// Get all the channels in the course.
        /// </summary>
        /// <param name="courseId">Course id.</param>
        /// <returns>List of channels.</returns>
        Task<IEnumerable<Channel>> GetAllChannelsAsync(string courseId);
    }
}