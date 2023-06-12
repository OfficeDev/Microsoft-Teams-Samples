// <copyright file="ActionBase.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace InMeetingNotificationsBot.Models
{
    public class ActionBase
    {
        /// <summary>
        ///  Model entity for identifying action type of card.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///  Model entity for extracting user's choiceset.
        /// </summary>
        public string Choice { get; set; }
    }
}
