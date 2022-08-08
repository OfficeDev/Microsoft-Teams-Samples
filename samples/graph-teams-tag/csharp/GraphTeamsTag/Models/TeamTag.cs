// <copyright file="TeamTag.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Models
{
    public class TeamTag
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public int MembersCount { get; set; }
    }
}
