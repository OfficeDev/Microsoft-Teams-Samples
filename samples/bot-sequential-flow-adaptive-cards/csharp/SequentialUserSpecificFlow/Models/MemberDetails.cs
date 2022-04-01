// <copyright file="MemberDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SequentialUserSpecificFlow.Models
{
    /// <summary>
    /// Member details model class.
    /// </summary>
    public class MemberDetails
    {
        public Info info { get; set; }
    }

    /// <summary>
    /// Info model class.
    /// </summary>
    public class Info
    {
        // Aad object id of member.
        public string value { get; set; }

        // User name of member.
        public string title { get; set; }
    }
}
