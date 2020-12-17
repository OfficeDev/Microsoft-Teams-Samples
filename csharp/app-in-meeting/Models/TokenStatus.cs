// <copyright file="TokenStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Models
{
    /// <summary>
    /// Token status type.
    /// </summary>
    public enum TokenStatus
    {
        /// <summary>
        /// This represents the waiting status.
        /// User with this token status is waiting in the list ot be serviced, their token number
        /// being greater than the current token being serviced.
        /// </summary>
        Waiting,

        /// <summary>
        /// This represents the current status.
        /// User with this token status is the current user being serviced. Their token number
        /// is the current token in the meeting.
        /// </summary>
        Current,

        /// <summary>
        /// This represents the serviced status.
        /// User with this token status have been serviced already, their token number
        /// is less than the current token being serviced.
        /// </summary>
        Serviced,

        /// <summary>
        /// This represents the not used status.
        /// User with this token status have been skipped by the organizer or
        /// have voluntarily given up the token.
        /// </summary>
        NotUsed,
    }
}
