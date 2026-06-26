// <copyright file="RequestDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

using System;

namespace BotRequestApproval.Models
{
    public class RequestDetails
    {
        public Guid TaskId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string UserName { get; set; }

        public string ManagerName { get; set; }

        public string UserCardId { get; set; }

        public string ManagerCardId { get; set; }
        public string Status { get; set; }
    }
}
