// <copyright file="SaveTaskDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace MessagingExtensionReminder.Models
{
    /// <summary>
    /// Save task detail model class.
    /// </summary>
    public class SaveTaskDetail
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTimeOffset DateTime { get; set; }
    }
}
