// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    public enum DocumentState
    {
        /// <summary>
        /// Active - when a document is created.
        /// </summary>
        Active,

        /// <summary>
        /// Complete - when all the signers complete signing a document
        /// </summary>
        Complete,
    }
}
