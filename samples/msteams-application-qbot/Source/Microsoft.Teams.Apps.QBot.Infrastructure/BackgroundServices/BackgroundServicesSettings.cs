// <copyright file="BackgroundServicesOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background services settings.
    /// </summary>
    public class BackgroundServicesSettings
    {
        /// <summary>
        /// Gets or sets publish kb setting.
        /// </summary>
        public int PublishKbFrequencyInMinutes { get; set; }

        /// <summary>
        /// Gets or sets delete user data frequency.
        /// </summary>
        public int DeleteUserDataFrequencyInDays { get; set; }
    }
}
