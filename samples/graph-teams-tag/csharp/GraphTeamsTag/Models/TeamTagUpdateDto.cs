// <copyright file="TeamTagUpdateDto.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace GraphTeamsTag.Models
{
    public class TeamTagUpdateDto : TeamTag
    {
        public IEnumerable<TeamTagMember> MembersToBeAdded { get; set; }
        public IEnumerable<TeamTagMember> MembersToBeDeleted { get; set; }
    }
}
