// <copyright file="BroadcastInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>
using System;

namespace ActivityFeedBroadcast.Model
{
    // Class with broadcast info model.
    public class BroadcastInfo
    {
        public Guid taskId { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string userId { get; set; }

        public string userName { get; set; }

        public string access_token { get; set; }
    }
}