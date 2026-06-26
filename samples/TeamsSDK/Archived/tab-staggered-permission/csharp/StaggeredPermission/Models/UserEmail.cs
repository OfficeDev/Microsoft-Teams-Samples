// <copyright file="UserEmail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace StaggeredPermission.Models
{
    /// <summary>
    /// User email model class.
    /// </summary>
    public class UserEmail
    {
        public string FromMail { get; set; }

        public string ToMail { get; set; }

        public string Subject { get; set; }

        public string Time { get; set; }
    }
}
