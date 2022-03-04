// <copyright file="ParticipantRole.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    /// <summary>
    /// Participant role types.
    /// </summary>
    public enum ParticipantRole
    {
        /// <summary>
        /// Attendee.
        /// </summary>
        Attendee = 0,

        /// <summary>
        /// Presenter.
        /// </summary>
        Presenter = 1,

        /// <summary>
        /// Producer.
        /// </summary>
        Producer = 2,

        /// <summary>
        /// Unknown Future Value.
        /// </summary>
        UnknownFutureValue = 3,
    }
}
