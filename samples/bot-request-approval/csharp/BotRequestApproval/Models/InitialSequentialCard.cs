// <copyright file="InitialSequentialCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace BotRequestApproval.Models
{
    /// <summary>
    /// Initial Sequential Card model class.
    /// </summary>
    public class InitialSequentialCard
    {
        public Action action { get; set; }
        public string trigger { get; set; }
    }

    /// <summary>
    /// Action model class.
    /// </summary>
    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public Data data { get; set; }
        public string verb { get; set; }
    }

    /// <summary>
    /// Data model class.
    /// </summary>
    public class Data
    {
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public List<string> UserId { get; set; }
        public string RequestTitle { get; set; }
        public string RequestDescription { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public string UserMRI { get; set; }
    }
}
