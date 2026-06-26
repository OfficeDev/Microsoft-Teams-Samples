// <copyright file="ICardFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.UserSpecificViews.Cards
{
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.UserSpecificViews.Bot;

    /// <summary>
    /// Card factory contract.
    ///
    /// Provides methods to create different ACs.
    /// </summary>
    public interface ICardFactory
    {
        /// <summary>
        /// Gets an adaptive card with auto refresh configured for all users.
        /// Note: This works when the total number of users in the chat/channel are <= 60.
        /// Otherwise, users will have to manually trigger refresh.
        /// </summary>
        /// <param name="cardType">Card type.</param>
        /// <returns></returns>
        Attachment GetAutoRefreshForAllUsersBaseCard(string cardType);

        /// <summary>
        /// Gets an adaptive card with auto refresh trigger configured for a single user.
        /// Note: It is possible for other users to trigger a refresh invoke manually.
        /// </summary>
        /// <param name="userMri">User Mri.</param>
        /// <param name="cardType">Card Type.</param>
        /// <returns>Card attachment.</returns>
        Attachment GetAutoRefreshForSpecificUserBaseCard(string userMri, string cardType);

        /// <summary>
        /// Gets an adaptive card with updated count specific for a user.
        /// </summary>
        /// <param name="userMri">User Mri.</param>
        /// <param name="actionData">Refresh action data.</param>
        /// <returns>Card attachment.</returns>
        Attachment GetUpdatedCardForUser(string userMri, RefreshActionData actionData);

        /// <summary>
        /// Gets an adaptive card with no further refreshes possible.
        /// </summary>
        /// <param name="actionData">Refresh action data.</param>
        /// <returns>Card attachment.</returns>
        Attachment GetFinalBaseCard(RefreshActionData actionData);

        /// <summary>
        /// Gets an adaptive card with options to choose from "For All" or "Me" card type.
        /// </summary>
        /// <returns>Card attachment.</returns>
        Attachment GetSelectCardTypeCard();
    }
}
