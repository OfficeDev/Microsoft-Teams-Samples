// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

namespace TabExternalAuth.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// User's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// User's profile photo url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// User's email details.
        /// </summary>
        public string Value { get; set; }
    }
}