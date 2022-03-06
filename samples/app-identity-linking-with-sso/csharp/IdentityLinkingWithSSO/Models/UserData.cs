// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using Microsoft.Graph;

namespace IdentityLinkingWithSSO.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class UserData
    {
        public User User { get; set; }

        public string Photo { get; set; }

        public string Title { get; set; }

        public string DisplayName { get; set; }

        public string Url { get; set; }

        public string Value { get; set; }
    }
}
