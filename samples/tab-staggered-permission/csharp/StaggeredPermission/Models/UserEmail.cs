// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using Microsoft.Graph;

namespace StaggeredPermission.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class UserEmail
    {
        public string FromMail { get; set; }

        public string ToMail { get; set; }

        public string Subject { get; set; }

        public string Time { get; set; }
    }
}
