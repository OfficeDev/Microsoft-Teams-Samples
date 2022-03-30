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
        public string value { get; set; }

        public string title { get; set; }
    }
}
