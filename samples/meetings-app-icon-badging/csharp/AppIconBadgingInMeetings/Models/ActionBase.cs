// <copyright file="ActionBase.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace AppIconBadgingInMeetings.Models
{
    /// <summary>
    /// Model entity for identifying action type of card and extracting user's choiceset.
    /// </summary>
    public class ActionBase
    {
        /// <summary>
        /// Gets or sets the action type of the card.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the user's choiceset.
        /// </summary>
        public string Choice { get; set; }
    }
}
