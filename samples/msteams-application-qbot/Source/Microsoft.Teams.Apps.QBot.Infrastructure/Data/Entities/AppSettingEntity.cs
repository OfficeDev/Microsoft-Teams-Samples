// <copyright file="AppSettingEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    /// <summary>
    /// App settings entity.
    /// </summary>
    internal class AppSettingEntity
    {
        /// <summary>
        /// Gets or sets key of the setting.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        public string Value { get; set; }
    }
}
