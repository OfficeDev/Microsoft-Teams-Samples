// <copyright file="RequestInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>
using System;

namespace TabRequestApproval.Model
{
    // Class with request info model.
    public class RequestInfo
    {
        public Guid taskId { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string userName { get; set; }

        public string personaName { get; set; }

        public string status { get; set; }

        public string access_token { get; set; }
    }
}