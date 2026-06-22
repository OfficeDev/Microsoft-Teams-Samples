// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace StaggeredPermission.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class UserData
    {
        public string Photo { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<UserEmail> Details { get; set; }
    }
}
