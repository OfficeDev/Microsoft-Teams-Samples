// <copyright file="UserData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyrightusing Microsoft.Graph;

using Microsoft.Graph;

namespace AppCompleteAuth.Models
{
    public class UserData
    {
        public User User { get; set; }

        public string Photo { get; set; }

        public string Title { get; set; }
    }
}
