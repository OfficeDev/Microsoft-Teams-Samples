// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using Microsoft.Graph;
using System.Collections.Generic;

namespace StaggeredPermission.Models
{
    /// <summary>
    /// User data model class.
    /// </summary>
    public class UserData
    {
        public User User { get; set; }

        public string Photo { get; set; }

        public string Title { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<UserEmail> Details { get; set; }
    }
}
